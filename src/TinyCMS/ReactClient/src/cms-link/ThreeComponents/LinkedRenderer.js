import React from 'react';
import { createLinkWrapper } from 'react-cms-link';
import 'aframe';
import 'aframe-animation-component';
import 'aframe-particle-system-component';
//import 'babel-polyfill';
import { Entity, Scene } from 'aframe-react';
//import ReactDOM from 'react-dom';


class LinkedRenderer extends React.Component {
    constructor(props) {
        super(props);
        this.state = { color: 'red' };
    }

    changeColor() {
        const colors = ['red', 'orange', 'yellow', 'green', 'blue'];
        this.setState({
            color: colors[Math.floor(Math.random() * colors.length)]
        });
    }

    render() {
        
        return (
            <Scene>
                <a-assets>
                    <img id="groundTexture" src="https://cdn.aframe.io/a-painter/images/floor.jpg" />
                    <img id="skyTexture" src="https://cdn.aframe.io/a-painter/images/sky.jpg" />
                </a-assets>
                {this.props.children}
                <Entity primitive="a-plane" src="#groundTexture" rotation="-90 0 0" height="100" width="100" />
                <Entity primitive="a-light" type="ambient" color="#445451" />
                <Entity primitive="a-light" type="point" intensity="2" position="2 4 4" />
                <Entity primitive="a-sky" height="2048" radius="30" src="#skyTexture" theta-length="90" width="2048" />
                {/* <Entity particle-system={{ preset: 'snow', particleCount: 2000 }} /> */}
                <Entity text={{ value: 'Hello, A-Frame React!', align: 'center' }} position={{ x: 0, y: 2, z: -1 }} />

                <Entity primitive="a-camera">
                    <Entity primitive="a-cursor" animation__click={{ property: 'scale', startEvents: 'click', from: '0.1 0.1 0.1', to: '1 0 1', dur: 150 }} />
                </Entity>
            </Scene>
        );
    }
}


export default createLinkWrapper(LinkedRenderer, ({ width, height }) => ({ width, height }));

