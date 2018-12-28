﻿using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using TinyCMS;
using TinyCMS.Interfaces;
using TinyCMS.SocketServer;

public class SocketConnectionHandler
{
    private readonly IContainer container;
    private readonly WebSocket socket;
    private readonly INodeSerializer serializer;
    private readonly INodeTypeFactory factory;

    private bool IsOpen => !socket.CloseStatus.HasValue;

    public SocketConnectionHandler(IContainer container, WebSocket socket, INodeSerializer serializer, INodeTypeFactory factory)
    {
        this.container = container;
        this.socket = socket;
        this.serializer = serializer;
        this.factory = factory;
        ConnectChangeHandlers();
        SendInitialData();
    }

    private void SendNode(INode node, int depth = 3)
    {
        if (IsOpen)
        {
            var dataToSend = serializer.ToArraySegment(
                node: container.RootNode,
                token: CurrentToken,
                depth: 3,
                level: 0,
                fetchRelations: true
            );

            socket.SendAsync(
                buffer: dataToSend,
                messageType: WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken: CancellationToken.None
            );
        }
    }

    private void SendInitialData()
    {
        SendNode(container.RootNode, depth : 20);
    }

    private string CurrentToken { get; set; }

    /// <summary>
    /// Listen for commands from the client, which can be authorization
    /// or node mutations.
    /// </summary>
    /// <returns></returns>
    public async Task ListenForCommands()
    {
        var buffer = new byte[1024 * 8];
        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!result.CloseStatus.HasValue)
        {
            ArraySegment<byte> segment = null;

            if (result.Count > 1)
            {
                var parsedRequest = SocketRequest.Parse(buffer, result.Count);
                if (parsedRequest.RequestType == RequestTypeEnum.AuthToken)
                {
                    CurrentToken = parsedRequest.Data;
                }
                else
                {
                    var returnData = container.MatchRequest(parsedRequest, factory);

                    SendNode(returnData);
                }
            }

            result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        RemoveChangeAction();
        await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }

    private void RemoveChangeAction()
    {
        container.OnValueChanged -= Container_OnValueChanged;
        container.OnChildrenChanged -= Container_OnChildrenChanged;
    }

    // TODO: does nothing
    void Container_OnChildrenChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        string parentNodeId = string.Empty;
        //foreach(var node in e.NewItems.OfType<INode>())
        //{
        //    parentNodeId = node.ParentId;
        //    SendNode(node);
        //}
        if (!string.IsNullOrEmpty(parentNodeId))
        {
            var parent = container.GetById(parentNodeId);
            if (parent != null)
                SendNode(parent);
        }
    }

    private void ConnectChangeHandlers()
    {
        container.OnValueChanged += Container_OnValueChanged;
        container.OnChildrenChanged += Container_OnChildrenChanged;
    }

    void Container_OnValueChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is INode node)
        {
            SendNode(node);
        }
    }

}
