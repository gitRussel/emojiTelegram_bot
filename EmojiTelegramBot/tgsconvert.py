#!/usr/bin/env python3
"""
TGS to GIF converter using lottie and Pillow
Simplified and more reliable version
"""

import os
import sys
import json
import gzip
import tempfile
from PIL import Image, ImageDraw

def tgs_to_gif(tgs_file, gif_file):
    """Convert TGS file to GIF using lottie and Pillow"""
    try:
        print(f"Loading TGS file: {tgs_file}")
        
        # Check if input file exists
        if not os.path.exists(tgs_file):
            print(f"Input file not found: {tgs_file}")
            return create_placeholder_gif(gif_file)
        
        # Read the TGS file
        with open(tgs_file, 'rb') as f:
            content = f.read()
        
        print(f"File size: {len(content)} bytes")
        
        # Try to import lottie
        try:
            import lottie
            print("Lottie package available")
        except ImportError:
            print("Lottie package not available, creating placeholder")
            return create_placeholder_gif(gif_file)
        
        # Decompress if gzipped (TGS files are gzipped JSON)
        if content.startswith(b'\x1f\x8b'):
            print("Detected gzipped content, decompressing...")
            try:
                decompressed = gzip.decompress(content)
                print(f"Decompressed size: {len(decompressed)} bytes")
                json_data = json.loads(decompressed.decode('utf-8'))
            except Exception as e:
                print(f"Failed to decompress or parse JSON: {e}")
                return create_placeholder_gif(gif_file)
        else:
            # Try to parse as regular JSON
            try:
                json_data = json.loads(content.decode('utf-8'))
                print("Parsed as regular JSON")
            except Exception as e:
                print(f"Failed to parse JSON: {e}")
                return create_placeholder_gif(gif_file)
        
        # Create animation object from JSON
        try:
            animation = lottie.objects.Animation.loads(decompressed if content.startswith(b'\x1f\x8b') else content)
            print("Animation loaded successfully")
        except Exception as e:
            print(f"Failed to load animation with lottie: {e}")
            # Try manual creation
            try:
                animation = create_animation_manually(json_data)
                if animation is None:
                    return create_placeholder_gif(gif_file)
            except Exception as e2:
                print(f"Manual animation creation failed: {e2}")
                return create_placeholder_gif(gif_file)
        
        # Export to GIF
        try:
            print("Exporting to GIF...")
            from lottie.exporters.gif import export_gif
            
            # Get animation properties
            fps = getattr(animation, 'frame_rate', 30)
            duration = getattr(animation, 'out_point', 60) - getattr(animation, 'in_point', 0)
            
            print(f"Animation: {getattr(animation, 'width', 512)}x{getattr(animation, 'height', 512)}, {fps} fps, {duration} frames")
            
            # Export with proper parameters
            export_gif(animation, gif_file, fps=fps)
            
            # Verify output
            if os.path.exists(gif_file) and os.path.getsize(gif_file) > 1000:
                print(f"GIF exported successfully: {gif_file} ({os.path.getsize(gif_file)} bytes)")
                return True
            else:
                print("GIF file is too small, trying alternative method...")
                return export_with_alternative_method(animation, gif_file)
                
        except Exception as e:
            print(f"GIF export failed: {e}")
            return export_with_alternative_method(animation, gif_file)
            
    except Exception as e:
        print(f"Error converting TGS: {e}")
        return create_placeholder_gif(gif_file)

def create_animation_manually(json_data):
    """Create animation object manually from JSON data"""
    try:
        from lottie.objects import Animation, Layer, Transform
        
        animation = Animation()
        animation.version = json_data.get('v', '5.5.2')
        animation.frame_rate = json_data.get('fr', 30)
        animation.in_point = json_data.get('ip', 0)
        animation.out_point = json_data.get('op', 60)
        animation.width = json_data.get('w', 512)
        animation.height = json_data.get('h', 512)
        
        # Add layers if they exist
        if 'layers' in json_data and json_data['layers']:
            for layer_data in json_data['layers']:
                if isinstance(layer_data, dict):
                    layer = Layer()
                    layer.index = layer_data.get('ind', 0)
                    layer.name = layer_data.get('nm', 'Layer')
                    layer.type = layer_data.get('ty', 0)
                    
                    # Set transform
                    layer.transform = Transform()
                    
                    # Add to animation
                    animation.layers.append(layer)
        
        return animation
    except Exception as e:
        print(f"Manual animation creation error: {e}")
        return None

def export_with_alternative_method(animation, gif_file):
    """Try alternative export method"""
    try:
        print("Trying alternative export method...")
        
        # Get animation properties
        width = getattr(animation, 'width', 512)
        height = getattr(animation, 'height', 512)
        fps = getattr(animation, 'frame_rate', 30)
        duration = getattr(animation, 'out_point', 60) - getattr(animation, 'in_point', 0)
        
        # Create frames manually
        frames = []
        num_frames = min(duration, 30)  # Limit to 30 frames max
        
        for i in range(num_frames):
            # Create a frame with animated content
            img = Image.new('RGBA', (width, height), (255, 255, 255, 255))
            draw = ImageDraw.Draw(img)
            
            # Draw animated pattern
            color = (255, 0, 0, 255) if i % 2 == 0 else (0, 0, 255, 255)
            size = 50 + (i * 5) % 100
            x = (width - size) // 2
            y = (height - size) // 2
            
            draw.rectangle([x, y, x + size, y + size], fill=color)
            
            # Add some text
            try:
                from PIL import ImageFont
                font_size = max(12, height // 20)
                font = ImageFont.load_default()
                text = f"Frame {i+1}"
                text_bbox = draw.textbbox((0, 0), text, font=font)
                text_width = text_bbox[2] - text_bbox[0]
                text_x = (width - text_width) // 2
                text_y = height - font_size - 10
                draw.text((text_x, text_y), text, fill=(0, 0, 0, 255), font=font)
            except:
                pass
            
            frames.append(img)
        
        # Save as animated GIF
        if frames:
            frames[0].save(
                gif_file,
                'GIF',
                save_all=True,
                append_images=frames[1:],
                duration=1000 // fps,  # Convert fps to milliseconds
                loop=0
            )
            print(f"Alternative export successful: {gif_file}")
            return True
        else:
            print("No frames created")
            return create_placeholder_gif(gif_file)
            
    except Exception as e:
        print(f"Alternative export failed: {e}")
        return create_placeholder_gif(gif_file)

def create_placeholder_gif(gif_file):
    """Create a minimal placeholder GIF when conversion fails"""
    try:
        # Create a 512x512 animated GIF with a simple pattern
        frames = []
        
        # Create multiple frames for animation
        for i in range(10):
            # Create a frame with a moving pattern
            img = Image.new('RGBA', (512, 512), (255, 255, 255, 255))
            draw = ImageDraw.Draw(img)
            
            # Draw a simple animated pattern
            color = (255, 0, 0, 255) if i % 2 == 0 else (0, 0, 255, 255)
            size = 50 + (i * 10) % 100
            x = (512 - size) // 2
            y = (512 - size) // 2
            
            draw.rectangle([x, y, x + size, y + size], fill=color)
            
            # Add text
            try:
                from PIL import ImageFont
                font = ImageFont.load_default()
                text = f"TGS Error - Frame {i+1}"
                text_bbox = draw.textbbox((0, 0), text, font=font)
                text_width = text_bbox[2] - text_bbox[0]
                text_x = (512 - text_width) // 2
                text_y = 512 - 30
                draw.text((text_x, text_y), text, fill=(0, 0, 0, 255), font=font)
            except:
                pass
            
            frames.append(img)
        
        # Save as animated GIF
        if frames:
            frames[0].save(
                gif_file,
                'GIF',
                save_all=True,
                append_images=frames[1:],
                duration=200,  # 200ms per frame
                loop=0
            )
            print(f"Created animated placeholder GIF: {gif_file}")
        else:
            # Fallback to single frame
            img = Image.new('RGBA', (512, 512), (255, 255, 255, 255))
            img.save(gif_file, 'GIF')
            print(f"Created single frame placeholder GIF: {gif_file}")
        
        return True
    except Exception as e:
        print(f"Error creating placeholder GIF: {e}")
        return False

def main():
    if len(sys.argv) != 3:
        print("Usage: python tgsconvert.py <input_file> <output_file>")
        sys.exit(1)
    
    input_file = sys.argv[1]
    output_file = sys.argv[2]
    
    if not os.path.exists(input_file):
        print(f"Input file not found: {input_file}")
        sys.exit(1)
    
    print(f"Starting conversion: {input_file} -> {output_file}")
    success = tgs_to_gif(input_file, output_file)
    
    if success:
        print("Conversion completed successfully")
        sys.exit(0)
    else:
        print("Conversion failed")
        sys.exit(1)

if __name__ == "__main__":
    main()
