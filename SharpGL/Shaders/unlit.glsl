[Shader comment]

++++++++
Camera
++++++++

[Shader vertex]
#version 150 core

in vec3 _pos;
in vec3 _normal;
in vec2 _texCoord;
out vec2 vTexCoord;

uniform mat4 _modelViewProjection;

void main() {
    gl_Position = _modelViewProjection * vec4(_pos, 1);
	vTexCoord = _texCoord;
}


[Shader fragment]
#version 150 core
#define PI 3.1415926535897932384626433832795

in vec2 vTexCoord;
out vec4 outColor;

uniform int _samplerCount;
uniform sampler2D _tex;
uniform float _time;
uniform vec4 _color;

void main() {
	if(_samplerCount > 0)
	{
		vec2 ts = textureSize(_tex, 0);
		outColor = texture2D(_tex, vTexCoord);
	}
	else
	{
		outColor = _color;
	}
}


