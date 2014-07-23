[Shader comment]

++++++++
Text drawing (Gui)
++++++++

[Shader vertex]
#version 120
void main()
{
    gl_Position = gl_Vertex;
	gl_TexCoord[0] = gl_MultiTexCoord0;
}


[Shader fragment]
#version 120
uniform sampler2D _tex;
uniform vec4 _color;
//

void main() {
	vec4 t = texture2D(_tex, gl_TexCoord[0].xy);
	gl_FragColor = vec4(_color.r * t.r, _color.g * t.g, _color.b * t.b, _color.a * t.a);
}