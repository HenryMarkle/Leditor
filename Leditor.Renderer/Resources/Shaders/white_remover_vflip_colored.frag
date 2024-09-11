#version 330

uniform sampler2D inputTexture;

in vec4 fragColor;
in vec2 fragTexCoord;

out vec4 FragColor;

void main()
{
    vec4 white = vec4(1.0, 1.0, 1.0, 1.0);

    vec4 c = texture(inputTexture, vec2(fragTexCoord.x, 1.0 - fragTexCoord.y));

    if (c == white) discard;

    FragColor = fragColor;
}
