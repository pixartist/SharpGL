[Shader comment]

++++++++
Lit & Textured
++++++++

[Shader vertex]
#version 150 core

in vec3 _pos;
in vec3 _normal;
in vec2 _texCoord;
out vec2 vTexCoord;
out vec3 vNormal;
uniform mat4 _modelViewProjection;
uniform mat4 _rotationMatrix;
void main() {
    gl_Position = _modelViewProjection * vec4(_pos, 1);
	vNormal = (_rotationMatrix * vec4(_normal, 1)).xyz;
	vTexCoord = _texCoord;
}


[Shader fragment]
#version 150 core
#define PI 3.1415926535897932384626433832795

in vec2 vTexCoord;
in vec3 vNormal;
out vec4 outColor;

uniform int _samplerCount;
uniform sampler2D _tex;
uniform float _time;
uniform vec4 _color;
uniform vec3 _ambient;
uniform vec3 _skylightColor;
uniform vec3 _skylightDirection;
void main() 
{
	vec4 c;
	if(_samplerCount > 0)
	{
		c = texture2D(_tex, vTexCoord);
	}
	else
	{
		c = _color;
	}
	vec3 light = _ambient + _skylightColor * (
		dot(
			normalize(_skylightDirection), normalize(vNormal)
		) + 1.0 
	) * 0.5;
	outColor = vec4(c.rgb * clamp(light, 0.0, 1.0), c.a); 
}


