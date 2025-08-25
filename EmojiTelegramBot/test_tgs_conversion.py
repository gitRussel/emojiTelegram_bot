#!/usr/bin/env python3
"""
Test script for TGS to GIF conversion
"""

import os
import sys
import subprocess

def test_tgs_conversion():
    """Test TGS to GIF conversion"""
    print("Testing TGS to GIF conversion...")
    
    # Check if tgsconvert.py exists
    script_path = "tgsconvert.py"
    if not os.path.exists(script_path):
        print(f"Error: {script_path} not found")
        return False
    
    # Check Python dependencies
    try:
        import lottie
        print("✓ lottie package available")
    except ImportError:
        print("✗ lottie package not available")
        return False
    
    try:
        from PIL import Image
        print("✓ Pillow package available")
    except ImportError:
        print("✗ Pillow package not available")
        return False
    
    # Test with a simple TGS file if available
    test_files = [
        "test.tgs",
        "sample.tgs",
        "emoji.tgs"
    ]
    
    for test_file in test_files:
        if os.path.exists(test_file):
            print(f"\nTesting with {test_file}...")
            output_file = test_file.replace('.tgs', '_test.gif')
            
            try:
                # Run conversion
                result = subprocess.run([
                    sys.executable, script_path, test_file, output_file
                ], capture_output=True, text=True, timeout=30)
                
                print(f"Exit code: {result.returncode}")
                if result.stdout:
                    print("Output:", result.stdout)
                if result.stderr:
                    print("Error:", result.stderr)
                
                # Check output file
                if os.path.exists(output_file):
                    size = os.path.getsize(output_file)
                    print(f"Output file created: {output_file} ({size} bytes)")
                    if size > 1000:
                        print("✓ Conversion successful!")
                        return True
                    else:
                        print("✗ Output file too small")
                else:
                    print("✗ Output file not created")
                    
            except subprocess.TimeoutExpired:
                print("✗ Conversion timed out")
            except Exception as e:
                print(f"✗ Error: {e}")
    
    print("\nNo test files found or all tests failed")
    return False

if __name__ == "__main__":
    success = test_tgs_conversion()
    sys.exit(0 if success else 1)
