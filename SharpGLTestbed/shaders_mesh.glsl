[Shader vertex]
#version 150 core
in vec3 pos;
in vec3 color;
uniform float _time;
uniform mat4 _modelViewProjection;


out float vTime;
out vec3 vColor;
out mat4 vProjection;
void main() {
    gl_Position = vec4(pos, 1.0);
	vColor = color;
	//vTime = _time;
	//vProjection = _modelViewProjection;
}




[Shader geometry]
#version 150 core
#define PI 3.1415926535897932384626433832795
#define BlockSize 0.1
layout(points) in;
layout(triangle_strip, max_vertices = 18) out;

in float vTime[];
in vec3 vColor[]; // Output from vertex shader for each vertex
in mat4 vProjection[];

uniform float _time;
uniform mat4 _modelViewProjection;

out vec3 fColor; // Output to fragment shader

void makeVertex(vec3 shift, mat4 rotation)
{
	gl_Position = _modelViewProjection * (gl_in[0].gl_Position + vec4(shift, 0.0) * rotation);
    EmitVertex();	
}
void createCube(float sizeHalf, mat4 rot)
{
	float p = 1;
	float n = -1;
	//front
	fColor = vColor[0];
	makeVertex(vec3(p * sizeHalf, -sizeHalf, -sizeHalf), rot);
	makeVertex(vec3(n * sizeHalf, -sizeHalf, -sizeHalf), rot);


	makeVertex(vec3(p * sizeHalf,  sizeHalf, -sizeHalf), rot);
	makeVertex(vec3(n * sizeHalf,  sizeHalf, -sizeHalf), rot);


	makeVertex(vec3(p * sizeHalf,  sizeHalf,  sizeHalf), rot);
	makeVertex(vec3(n * sizeHalf,  sizeHalf,  sizeHalf), rot);


	makeVertex(vec3(p * sizeHalf, -sizeHalf,  sizeHalf), rot);
	makeVertex(vec3(n * sizeHalf, -sizeHalf,  sizeHalf), rot);


	makeVertex(vec3(p * sizeHalf, -sizeHalf, -sizeHalf), rot);
	makeVertex(vec3(n * sizeHalf, -sizeHalf, -sizeHalf), rot);
	EndPrimitive();


	makeVertex(vec3( sizeHalf, -sizeHalf, p * sizeHalf), rot);
	makeVertex(vec3( sizeHalf, -sizeHalf, n * sizeHalf), rot);

	makeVertex(vec3( sizeHalf,  sizeHalf, p * sizeHalf), rot);
	makeVertex(vec3( sizeHalf,  sizeHalf, n * sizeHalf), rot);
	EndPrimitive();


	makeVertex(vec3(-sizeHalf, -sizeHalf, n * sizeHalf), rot);
	makeVertex(vec3(-sizeHalf, -sizeHalf, p * sizeHalf), rot);

	makeVertex(vec3(-sizeHalf,  sizeHalf, n * sizeHalf), rot);
	makeVertex(vec3(-sizeHalf,  sizeHalf, p * sizeHalf), rot);
}

mat4 rotationMatrix(vec3 axis, float angle)
{
    axis = normalize(axis);
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;
    
    return mat4(oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,  0.0,
                oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,  0.0,
                oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c,           0.0,
                0.0,                                0.0,                                0.0,                                1.0);
}
void main() {
	
	mat4 rot = rotationMatrix(vec3(1,0,0), vTime[0]);
	createCube(0.8, rot);
    EndPrimitive();
}

[Shader fragment]
#version 150 core
#define PI 3.1415926535897932384626433832795
in vec3 fColor;
out vec4 outColor;
uniform float _time;
void main() {
	float s =  0.5 + sin(_time)/2;
	float c =  0.5 + cos(_time)/2;
	float t =  (0.4 + sin(_time) * cos(_time));
    outColor = vec4(fColor.r * s, fColor.g * c, fColor.b * t, 1.0);
}