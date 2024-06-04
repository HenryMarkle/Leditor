#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

uniform sampler2D inputTexture;

void main()
{
    vec4 color = texture(inputTexture, fragTexCoord);

    vec4 newColor = color;

    // if (color.r > 0.5) newColor.r = 0;
    // if (color.g > 0.5) newColor.g = 0;
    // if (color.b > 0.5) newColor.b = 0;

    FragColor = vec4(1 - color.r, 1 - color.g, 1 - color.b, 1);
}
