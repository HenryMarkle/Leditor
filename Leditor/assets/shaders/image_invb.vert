#version 330

// Input vertex attributes
in vec3 vertexPosition;
in vec2 vertexTexCoord;
in vec3 vertexNormal;
in vec4 vertexColor;

uniform mat4 mvp;

out vec2 fragTexCoord;
out vec4 fragColor;

uniform vec2 vertex_pos[4];

void main()
{
    fragTexCoord = vertexPosition.xy;
    
    fragColor = vertexColor;

    gl_Position = mvp*vec4(vertexPosition, 1.0);
}