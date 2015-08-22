/// TextureUtils 0.1
/// by nihilocrat <nihilocrat@gmail.com>
///
/// Some utility functions for procedurally generating or modifying textures at runtime
/// Just put this in your Assets/Plugins directory and call like so in any of your scripts:
/// "TextureUtils.Colorize(myPixels, myColor)"
///
/// licensed under the BSD license : http://creativecommons.org/licenses/BSD/

using UnityEngine;

public class TextureUtils {
	/// colorize a texture
	public static Color[] Colorize(Color[] pixels, Color tint) {
		for(int i=0; i < pixels.Length; i++) {				
			pixels[i].r = pixels[i].r - (1.0f - tint.r);
			pixels[i].g = pixels[i].g - (1.0f - tint.g);
			pixels[i].b = pixels[i].b - (1.0f - tint.b);
		}
		
		return pixels;
	}
	
	public static Color[] Colorize(Color[] pixels, Color gradientLow, Color gradientHigh) {
		for(int i=0; i < pixels.Length; i++) {
			Color tint = Color.Lerp(gradientLow, gradientHigh, pixels[i].r);
			pixels[i].r = pixels[i].r - (1.0f - tint.r);
			pixels[i].g = pixels[i].g - (1.0f - tint.g);
			pixels[i].b = pixels[i].b - (1.0f - tint.b);
		}
		
		return pixels;
	}
	
	public static Color[] ConvertToGradient(Color[] pixels, Color gradientLow, Color gradientHigh) {
		for(int i=0; i < pixels.Length; i++) {
			pixels[i] = Color.Lerp(gradientLow, gradientHigh, pixels[i].r);
		}
		
		return pixels;
	}
	
	/// mask a texture using a second texture
	public static Color[] Mask(Color[] pixels, Color[] maskPixels) {
		for(int i=0; i < pixels.Length; i++) {
			pixels[i].r *= maskPixels[i].r;
			pixels[i].g *= maskPixels[i].r;
			pixels[i].b *= maskPixels[i].r;
			
			pixels[i].a = maskPixels[i].a;
			if(pixels[i].a <= 0.0f) {
				pixels[i] = new Color(0,0,0,0);
			}
		}
		
		return pixels;
	}
	
	/// paste one texture on top of another
	public static Color[] Paste(Color[] topPixels, Color[] bottomPixels) {
		for(int i=0; i < bottomPixels.Length; i++) {
			bottomPixels[i] = Color.Lerp(bottomPixels[i], topPixels[i], topPixels[i].a);
		}
		
		return bottomPixels;
	}
}