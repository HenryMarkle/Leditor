#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

uniform sampler2D inputTexture;

void main()
{
    vec4 color = texture(inputTexture, fragTexCoord);

    if (color.r == 1.0 && color.g == 1.0 && color.b == 1.0) {
        discard;
    }

    FragColor = fragColor;
}
