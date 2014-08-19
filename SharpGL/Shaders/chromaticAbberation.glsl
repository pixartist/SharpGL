[Shader vertex]

#version 150 core

in vec2 _pos;
out vec2 texCoord;
uniform float _time;
uniform sampler2D _tex;

void main() {
    gl_Position = vec4(_pos, 0, 1);
	texCoord = _pos/2+vec2(0.5,0.5);
}
[Shader fragment]
#version 150 core

in vec2 texCoord;
out vec4 outColor;

uniform float _time;
uniform sampler2D _tex;
uniform float blur;
uniform float baseBlur;
uniform float chromatic;

float sqrt2 = sqrt(2.0);


vec4 effect(vec2 pr, vec2 pg, vec2 pb, vec2 size, float blurRadius)
{
	vec2 sr, sg, sb, d;
	vec4 r,g,b,a;
	vec2 p;
	vec4 color = vec4(0.0,0.0,0.0,0.0);
	int c = 0;
	
	for(p.x = -blurRadius; p.x <= blurRadius; p.x++)
	{
		for(p.y = -blurRadius; p.y <= blurRadius; p.y++)
		{
			d = p/size;
			sr = pr + d;
			sg = pg + d;
			sb = pb + d;
			r = texture2D(_tex, sr);
			g = texture2D(_tex, sg);
			b = texture2D(_tex, sb);
			color += vec4(r.r*r.a, g.g*g.a, b.b*b.a, 1.0);
			c++;
		}
	}
	return color/c;
}
void main()
{
	vec2 size = textureSize(_tex, 0);
	vec2 h = vec2(0.5, 0.5);
	vec2 pos = texCoord.xy - h;
	float dFromCenter = sqrt(pos.x*pos.x + pos.y*pos.y) / sqrt2;
	float cr = (dFromCenter*dFromCenter);
	vec2 pr = pos * (1-(chromatic*cr));
	vec2 pb = pos * (1+(chromatic*cr));
	pos += h;
	pr += h;
	pb += h;
	float bAmount = min(baseBlur + blur*dFromCenter, 16);
	outColor = effect(pr, pos, pb, size, bAmount) * (1-dFromCenter);
}