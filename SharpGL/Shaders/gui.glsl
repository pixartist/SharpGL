[Shader comment]

++++++++
Gui Render to Rect
++++++++

[Shader vertexRect]
#version 150 core

in vec2 pos;
out vec2 texCoord;
uniform float _time;
uniform sampler2D tex;

void main() {
    gl_Position = vec4(pos, 0, 1);
	texCoord = pos/2+vec2(0.5,0.5);
}


[Shader fragmentRect]
#version 150 core
#define PI 3.1415926535897932384626433832795

out vec4 outColor;
uniform float _time;
uniform sampler2D tex;
in vec2 texCoord;
//
void main() {
	outColor = texture2D(tex, texCoord);
}