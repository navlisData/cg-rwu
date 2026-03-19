#version 330

out vec4 outputColor;

in vec2 texCoord;

uniform sampler2D texture0;
uniform vec4 uOverrideColor;
uniform float uBlendFactor;

void main()
{
    vec4 texColor = texture(texture0, texCoord);
    outputColor = vec4(mix(texColor.rgb, uOverrideColor.rgb, uBlendFactor), texColor.a * uOverrideColor.a);
}