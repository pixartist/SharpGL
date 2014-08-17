[Shader comment]

++++++++
Screenbuffer
++++++++

[Shader vertex]
#version 150 core

in vec2 _pos;
out vec2[9] texCoords;
uniform float _time;
uniform sampler2D tex;
uniform float texelW;
uniform float texelH;
void main() {
    gl_Position = vec4(_pos, 0, 1);
	vec2 c = _pos/2+vec2(0.5,0.5);
	
	texCoords[0] = c + vec2(-texelW, -texelH);
	texCoords[1] = c + vec2(0, -texelH);
	texCoords[2] = c + vec2(texelW, -texelH);

	texCoords[3] = c + vec2(-texelW, 0);
	texCoords[4] = c + vec2(0, 0);
	texCoords[5] = c + vec2(texelW, 0);

	texCoords[6] = c + vec2(-texelW, texelH);
	texCoords[7] = c + vec2(0, texelH);
	texCoords[8] = c + vec2(texelW, texelH);
}


[Shader fragment]
#version 150 core
#define PI 3.1415926535897932384626433832795

out vec3 outColor;
uniform float _time;
uniform sampler2D tex;
in vec2[9] texCoords;
//
void main() {
	outColor = texture2D(tex, texCoords[4]);
}