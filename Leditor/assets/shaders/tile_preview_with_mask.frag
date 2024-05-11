#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

uniform sampler2D inputTexture;

uniform float heightStart;
uniform float height;
uniform float width;

void main()
{
    vec4 maskColor = texture(maskTexture, fragTexCoord);

    if (maskColor.r == 1.0 && maskColor.g == 1.0 && maskColor.b == 1.0) {
        discard;
    }
    
    vec2 newCoord = vec2(fragTexCoord.x * width, fragTexCoord.y * height + heightStart);
    vec4 color = texture(inputTexture, newCoord);

    if (color.r == 1.0 && color.g == 1.0 && color.b == 1.0) {
        discard;
    } else {
        FragColor = fragColor;
    }
}
