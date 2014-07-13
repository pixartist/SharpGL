[Shader comment]

++++++++
Camera
++++++++

[Shader vertex]
#version 150 core

in vec3 pos;
in vec3 normal;
uniform float _time;
uniform mat4 _modelViewProjection;
uniform vec4 _color;
out vec4 vColor;
void main() {
	vColor = _color;
    gl_Position = _modelViewProjection * vec4(pos, 1);
	
}
[Shader fragment]
#version 150 core
#define PI 3.1415926535897932384626433832795

in vec4 vColor;
out vec4 outColor;

uniform float _time;
uniform vec4 _color;
void main() {
    outColor = _color;
}


