[Shader vertex]
#version 150 core

in vec3 pos;
in vec4 color;
uniform float _time;
uniform mat4 _modelViewProjection;

out vec4 vColor;

void main() {
    gl_Position =   _modelViewProjection * vec4(pos, 1);
	vColor = color;
}



[Shader fragment]
#version 150 core
#define PI 3.1415926535897932384626433832795

in vec4 vColor;

out vec4 outColor;
uniform float _time;
void main() {
	float s =  0.5 + sin(_time)/2;
	float c =  0.5 + cos(_time)/2;
	float t =  (0.4 + sin(_time) * cos(_time));
    outColor = vColor; //vec4(vColor.r * s, vColor.g * c, vColor.b * t, 1.0);
}