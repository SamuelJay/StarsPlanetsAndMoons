using System.IO;
using UnityEditor;
using UnityEngine;

namespace StarsPlanetsAndMoons.Editor {
    public static class PlaceholderTextures {
        public const string EarthPath = "Assets/Textures/EarthSurface.png";
        public const string CloudsPath = "Assets/Textures/EarthClouds.png";

        private const int Width = 1024;
        private const int Height = 512;

        public static void Generate(Vector3 homeDirection) {
            Directory.CreateDirectory(Path.GetDirectoryName(EarthPath));
            Write(EarthPath, BuildEarth(homeDirection), true);
            Write(CloudsPath, BuildClouds(), false);
        }

        private static Texture2D BuildEarth(Vector3 homeDirection) {
            Color deepOcean = new Color(0.04f, 0.16f, 0.42f);
            Color shallowOcean = new Color(0.1f, 0.35f, 0.62f);
            Color lowland = new Color(0.22f, 0.48f, 0.18f);
            Color highland = new Color(0.5f, 0.42f, 0.25f);
            Color ice = new Color(0.92f, 0.95f, 0.98f);

            Texture2D texture = new Texture2D(Width, Height, TextureFormat.RGBA32, true);
            Color[] pixels = new Color[Width * Height];
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    Vector3 direction = DirectionOf(x, y);
                    float angle = Vector3.Angle(direction, homeDirection) * Mathf.Deg2Rad;
                    float land = Fbm(direction * 1.8f, 5, 0f) + 0.4f * Mathf.SmoothStep(0f, 1f, 1f - angle / 0.8f);

                    Color color;
                    if (land > 0.52f) {
                        color = Color.Lerp(lowland, highland, Mathf.InverseLerp(0.58f, 0.75f, land));
                    } else {
                        color = Color.Lerp(deepOcean, shallowOcean, Mathf.InverseLerp(0.3f, 0.52f, land));
                    }

                    float latitude = Mathf.Abs(Mathf.Asin(direction.y)) * Mathf.Rad2Deg;
                    float iceNoise = Fbm(direction * 4f, 3, 20f) * 10f;
                    color = Color.Lerp(color, ice, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(72f, 78f, latitude + iceNoise)));
                    pixels[y * Width + x] = color;
                }
            }
            texture.SetPixels(pixels);
            return texture;
        }

        private static Texture2D BuildClouds() {
            Texture2D texture = new Texture2D(Width, Height, TextureFormat.RGBA32, true);
            Color[] pixels = new Color[Width * Height];
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    Vector3 direction = DirectionOf(x, y);
                    float density = Fbm(direction * 3.2f, 5, 100f);
                    float alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.5f, 0.68f, density)) * 0.9f;
                    pixels[y * Width + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            texture.SetPixels(pixels);
            return texture;
        }

        private static Vector3 DirectionOf(int x, int y) {
            float longitude = ((x + 0.5f) / Width - 0.5f) * 2f * Mathf.PI;
            float latitude = ((y + 0.5f) / Height - 0.5f) * Mathf.PI;
            return new Vector3(Mathf.Cos(latitude) * Mathf.Cos(longitude), Mathf.Sin(latitude), Mathf.Cos(latitude) * Mathf.Sin(longitude));
        }

        private static float Fbm(Vector3 p, int octaves, float offset) {
            float sum = 0f;
            float amplitude = 0.5f;
            float total = 0f;
            for (int i = 0; i < octaves; i++) {
                sum += amplitude * Noise(p + new Vector3(offset, offset * 0.7f, offset * 1.3f));
                total += amplitude;
                p *= 2.03f;
                amplitude *= 0.5f;
            }
            return sum / total;
        }

        private static float Noise(Vector3 p) {
            float xy = Mathf.PerlinNoise(p.x + 31.4f, p.y + 17.2f);
            float yz = Mathf.PerlinNoise(p.y + 5.1f, p.z + 83.7f);
            float zx = Mathf.PerlinNoise(p.z + 49.9f, p.x + 12.3f);
            return (xy + yz + zx) / 3f;
        }

        private static void Write(string path, Texture2D texture, bool opaque) {
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.alphaIsTransparency = !opaque;
            importer.wrapModeU = TextureWrapMode.Repeat;
            importer.wrapModeV = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }
    }
}
