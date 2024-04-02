#version 330

uniform vec2 tl;
uniform vec2 tr;
uniform vec2 br;
uniform vec2 bl;

// Input vertex attributes
in vec3 vertexPosition;
in vec2 vertexTexCoord;
in vec3 vertexNormal;
in vec4 vertexColor;

// Input uniform values
uniform mat4 mvp;

// Output vertex attributes (to fragment shader)
out vec2 fragTexCoord;
out vec4 fragColor;

// NOTE: Add here your custom variables

void main()
{
    // Send vertex attributes to fragment shader
    fragTexCoord = vertexTexCoord;
    fragColor = vertexColor;
    
    vec4 normalized = vec4(vertexPosition, 1) * mvp;
    
    // Calculate final vertex position
    gl_Position = vec4(mix(mix(bl,br,normalized.x), mix(tl,tr,normalized.x), normalized.y), 1, 1);
}