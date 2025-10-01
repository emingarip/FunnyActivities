/**
 * Image optimization utilities for mobile performance
 * Provides functions to compress, resize, and optimize images before upload
 */

export interface ImageOptimizationOptions {
  maxWidth?: number;
  maxHeight?: number;
  quality?: number;
  maxFileSize?: number; // in bytes
  format?: 'jpeg' | 'png' | 'webp';
}

export interface OptimizedImageResult {
  file: File;
  originalSize: number;
  optimizedSize: number;
  compressionRatio: number;
}

/**
 * Compresses and resizes an image file for optimal mobile performance
 */
export const optimizeImage = async (
  file: File,
  options: ImageOptimizationOptions = {}
): Promise<OptimizedImageResult> => {
  const {
    maxWidth = 1200,
    maxHeight = 1200,
    quality = 0.8,
    maxFileSize = 1024 * 1024, // 1MB default
    format = 'jpeg'
  } = options;

  return new Promise((resolve, reject) => {
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    const img = new Image();

    img.onload = () => {
      try {
        // Calculate new dimensions while maintaining aspect ratio
        let { width, height } = img;

        if (width > height) {
          if (width > maxWidth) {
            height = (height * maxWidth) / width;
            width = maxWidth;
          }
        } else {
          if (height > maxHeight) {
            width = (width * maxHeight) / height;
            height = maxHeight;
          }
        }

        // Set canvas dimensions
        canvas.width = width;
        canvas.height = height;

        // Draw and compress image
        ctx?.drawImage(img, 0, 0, width, height);

        // Convert to blob with specified format and quality
        canvas.toBlob(
          (blob) => {
            if (!blob) {
              reject(new Error('Failed to compress image'));
              return;
            }

            // If the compressed image is still too large, reduce quality further
            if (blob.size > maxFileSize && quality > 0.5) {
              const newQuality = Math.max(0.5, quality * 0.8);
              canvas.toBlob(
                (compressedBlob) => {
                  if (compressedBlob) {
                    const optimizedFile = new File([compressedBlob], file.name, {
                      type: `image/${format}`,
                      lastModified: Date.now(),
                    });

                    resolve({
                      file: optimizedFile,
                      originalSize: file.size,
                      optimizedSize: compressedBlob.size,
                      compressionRatio: (file.size - compressedBlob.size) / file.size,
                    });
                  } else {
                    reject(new Error('Failed to compress image further'));
                  }
                },
                `image/${format}`,
                newQuality
              );
            } else {
              const optimizedFile = new File([blob], file.name, {
                type: `image/${format}`,
                lastModified: Date.now(),
              });

              resolve({
                file: optimizedFile,
                originalSize: file.size,
                optimizedSize: blob.size,
                compressionRatio: (file.size - blob.size) / file.size,
              });
            }
          },
          `image/${format}`,
          quality
        );
      } catch (error) {
        reject(error);
      }
    };

    img.onerror = () => {
      reject(new Error('Failed to load image'));
    };

    // Create object URL for the image
    img.src = URL.createObjectURL(file);
  });
};

/**
 * Batch optimize multiple images
 */
export const optimizeImages = async (
  files: File[],
  options: ImageOptimizationOptions = {},
  onProgress?: (completed: number, total: number) => void
): Promise<OptimizedImageResult[]> => {
  const results: OptimizedImageResult[] = [];
  const total = files.length;

  for (let i = 0; i < files.length; i++) {
    try {
      const result = await optimizeImage(files[i], options);
      results.push(result);
      onProgress?.(i + 1, total);
    } catch (error) {
      console.error(`Failed to optimize image ${files[i].name}:`, error);
      // Return original file if optimization fails
      results.push({
        file: files[i],
        originalSize: files[i].size,
        optimizedSize: files[i].size,
        compressionRatio: 0,
      });
    }
  }

  return results;
};

/**
 * Get image dimensions without loading the full image
 */
export const getImageDimensions = (file: File): Promise<{ width: number; height: number }> => {
  return new Promise((resolve, reject) => {
    const img = new Image();
    img.onload = () => {
      resolve({ width: img.naturalWidth, height: img.naturalHeight });
      URL.revokeObjectURL(img.src);
    };
    img.onerror = () => {
      reject(new Error('Failed to get image dimensions'));
    };
    img.src = URL.createObjectURL(file);
  });
};

/**
 * Check if an image needs optimization
 */
export const needsOptimization = async (
  file: File,
  options: ImageOptimizationOptions = {}
): Promise<boolean> => {
  const { maxWidth = 1200, maxHeight = 1200, maxFileSize = 1024 * 1024 } = options;

  if (file.size > maxFileSize) {
    return true;
  }

  try {
    const dimensions = await getImageDimensions(file);
    return dimensions.width > maxWidth || dimensions.height > maxHeight;
  } catch {
    // If we can't get dimensions, assume it needs optimization
    return true;
  }
};

/**
 * Default optimization options for mobile devices
 */
export const MOBILE_OPTIMIZATION_OPTIONS: ImageOptimizationOptions = {
  maxWidth: 1200,
  maxHeight: 1200,
  quality: 0.8,
  maxFileSize: 1024 * 1024, // 1MB
  format: 'jpeg',
};

/**
 * Optimization options for thumbnails
 */
export const THUMBNAIL_OPTIMIZATION_OPTIONS: ImageOptimizationOptions = {
  maxWidth: 300,
  maxHeight: 300,
  quality: 0.7,
  maxFileSize: 100 * 1024, // 100KB
  format: 'jpeg',
};

// Export empty object to make this a module
export {};