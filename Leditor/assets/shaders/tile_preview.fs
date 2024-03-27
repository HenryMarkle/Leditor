#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

uniform sampler2D inputTexture;
uniform vec4 highlightColor;
//uniform float heightStart;
//uniform float height;

void main()
{
    vec4 color = texture(inputTexture, fragTexCoord);

    if (color.r == 1.0 && color.g == 1.0 && color.b == 1.0) {
        discard;
    } else {
        FragColor = vec4(highlightColor.r/255.0, highlightColor.g/255.0, highlightColor.b/255.0, highlightColor.a/255.0);
    }
}
