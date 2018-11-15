﻿using System;
using TinyCMS.Commerce.Models;
using System.Collections.Generic;
using System.Linq;
namespace TinyCMS.Commerce.Services
{

    public abstract class OrderService : IOrderService
    {
        public List<IOrder> ActiveOrders = new List<IOrder>();

        public void Delete(IOrder order)
        {
            DeleteOrderFromStorage(order);
        }

        public IOrder GetNewOrder()
        {
            var ret = Factory.Instance.CreateInstance<IOrder>();
            ActiveOrders.Add(ret);
            ret.PropertyChanged += OrderChanged;
            return ret;
        }

        void OrderChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SaveOrder(sender as IOrder);
        }

        internal abstract void SaveOrder(IOrder order);
        internal abstract void DeleteOrder(IOrder order);

        internal virtual void DeleteOrderFromStorage(IOrder order)
        {
            if (ActiveOrders.Contains(order))
                ActiveOrders.Remove(order);
            Delete(order);
        }

        public IOrder GetOrder(string id)
        {
            return GetOrderFromStorage(id);
        }

        private IOrder GetOrderFromStorage(string id)
        {
            return ActiveOrders.FirstOrDefault(d => d.Id.Equals(id));
            
        }
    }
}
