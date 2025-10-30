#version 330 core

in vec2 texCoord;
out vec4 outputColor;

uniform sampler2D texture0;
uniform vec4 shadowColor;

void main()
{
    float alpha = texture(texture0, texCoord).a;
    outputColor = vec4(shadowColor.rgb, shadowColor.a * alpha);
}