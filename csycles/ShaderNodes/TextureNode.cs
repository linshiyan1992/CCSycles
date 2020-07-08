﻿/**
Copyright 2014 Robert McNeel and Associates

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
**/

using ccl.Attributes;
using System;

namespace ccl.ShaderNodes
{
	/// <summary>
	/// Base texture node
	/// </summary>
	[ShaderNode("texture_node_base", true)]
	public class TextureNode : ShaderNode
	{
		/// <summary>
		/// Mapping type to transform according
		/// </summary>
		public enum MappingType : uint
		{
			/// <summary>
			/// Transform as point
			/// </summary>
			Point = 0,
			/// <summary>
			/// Transform texture space
			/// </summary>
			Texture,
			/// <summary>
			/// Transform vector
			/// </summary>
			Vector,
			/// <summary>
			/// Transform normal
			/// </summary>
			Normal
		}
		public enum TextureColorSpace
		{
			None,
			Color,
		}

		public enum TextureProjection
		{
			Flat,
			Box,
			Sphere,
			Tube,
		}

		public enum DecalDirection
		{
			Both = 0,
			Forward = 1,
			Backward = 2,
		}

		public enum TextureExtension
		{
			Repeat,
			Extend,
			Clip,
		}

		public enum EnvironmentProjection
		{
			Equirectangular,
			MirrorBall,
			Wallpaper,
			EnvironmentMap,
			Box,
			LightProbe,
			CubeMap,
			CubeMapHorizontal,
			CubeMapVertical,
			Hemispherical,
			Spherical,
		}

		public enum AxisMapping
		{
			None = 0,
			X = 1,
			Y = 2,
			Z = 3
		}

		internal TextureNode(ShaderNodeType type) :
			this(type, "texturenode baseclass") { }

		internal TextureNode(ShaderNodeType type, string name) :
			base(type, name) {
				Mapping = MappingType.Texture;
				UseMin = false;
				UseMax = false;
				Translation = new float4(0.0f);
				Rotation = new float4(0.0f);
				Scale = new float4(1.0f);
				Min = new float4(float.MinValue);
				Max = new float4(float.MaxValue);
		}

		/// <summary>
		/// Color space to operate in
		/// </summary>
		public TextureColorSpace ColorSpace { get; set; }
		/// <summary>
		/// EnvironmentTexture texture interpolation
		/// </summary>
		public InterpolationType Interpolation { get; set; }
		/// <summary>
		/// texture extension type.
		/// </summary>
		public TextureExtension Extension { get; set; }
		/// <summary>
		/// Set to true if image data is to be interpreted as linear.
		/// </summary>
		public bool IsLinear { get; set; }
		/// <summary>
		/// Get or set image name
		/// </summary>
		public string Filename { get; set; }
		/// <summary>
		/// Get or set the float data for image. Use for HDR images
		/// </summary>
		public float[] FloatImage { set; get; }
		public IntPtr FloatImagePtr { set; get; } = IntPtr.Zero;
		/// <summary>
		/// Get or set the byte data for image
		/// </summary>
		public byte[] ByteImage { set; get; }
		public IntPtr ByteImagePtr { set; get; } = IntPtr.Zero;
		/// <summary>
		/// Get or set image resolution width
		/// </summary>
		public uint Width { get; set; }
		/// <summary>
		/// Get or set image resolution height
		/// </summary>
		public uint Height { get; set; }

		/// <summary>
		/// Get or set the mapping type to use
		/// </summary>
		public MappingType Mapping { get; set; } = MappingType.Texture;
		/// <summary>
		/// Set to true if mapping output in Value should be floored to Min
		/// </summary>
		public bool UseMin { get; set; } = false;
		/// <summary>
		/// Set to true if mapping output in Value should be ceiled to Max
		/// </summary>
		public bool UseMax { get; set; } = false;
		/// <summary>
		/// Translate input vector with this
		/// </summary>
		public float4 Translation { get; set; } = new float4(0.0f);
		/// <summary>
		/// Rotate input vector with this
		/// </summary>
		public float4 Rotation { get; set; } = new float4(0.0f);
		/// <summary>
		/// Scale input vector with this
		/// </summary>
		public float4 Scale { get; set; } = new float4(1.0f);
		/// <summary>
		/// If UseMin is true, use this as minimum values for resulting vector
		/// </summary>
		public float4 Min { get; set; } = new float4(float.MinValue);
		/// <summary>
		/// If UseMax is true, use this as maximum values for resulting vector
		/// </summary>
		public float4 Max { get; set; } = new float4(float.MaxValue);

		/// <summary>
		/// Mapping of X axis, default to X
		/// </summary>
		public AxisMapping XMapping { get; set; } = AxisMapping.X;
		/// <summary>
		/// Mapping of Y axis, default to Y
		/// </summary>
		public AxisMapping YMapping { get; set; } = AxisMapping.Y;
		/// <summary>
		/// Mapping of Z axis, default to Z
		/// </summary>
		public AxisMapping ZMapping { get; set; } = AxisMapping.Z;

		protected void SetInterpolation(string interp)
		{
			interp = interp.Replace(" ", "_");
			Interpolation = (InterpolationType)Enum.Parse(typeof(InterpolationType), interp, true);
		}
		protected void SetExtension(string extension)
		{
			extension = extension.Replace(" ", "_");
			Extension = (TextureExtension)Enum.Parse(typeof(TextureExtension), extension, true);
		}
		protected void SetColorSpace(string cs)
		{
			ColorSpace = (TextureColorSpace)Enum.Parse(typeof(TextureColorSpace), cs, true);
		}

		internal override void SetDirectMembers(uint clientId, uint sceneId, uint shaderId)
		{
			CSycles.shadernode_set_member_bool(clientId, shaderId, Id, Type, "useminmax", UseMin || UseMax);
			if (UseMin)
			{
				CSycles.shadernode_set_member_vec(clientId, shaderId, Id, Type, "min", Min.x, Min.y, Min.z);
			}
			if (UseMax)
			{
				CSycles.shadernode_set_member_vec(clientId, shaderId, Id, Type, "max", Max.x, Max.y, Max.z);
			}
			var tr = Translation;
			CSycles.shadernode_texmapping_set_transformation(clientId, shaderId, Id, Type, 0, tr.x, tr.y, tr.z);
			var rt = Rotation;
			CSycles.shadernode_texmapping_set_transformation(clientId, shaderId, Id, Type, 1, rt.x, rt.y, rt.z);
			var sc = Scale;
			CSycles.shadernode_texmapping_set_transformation(clientId, shaderId, Id, Type, 2, sc.x, sc.y, sc.z);

			CSycles.shadernode_texmapping_set_mapping(clientId, shaderId, Id, Type, (uint)XMapping, (uint)YMapping, (uint)ZMapping);

			CSycles.shadernode_texmapping_set_type(clientId, shaderId, Id, Type, (uint)Mapping);

			CSycles.shadernode_set_member_bool(clientId, shaderId, Id, Type, "is_linear", IsLinear);
		}

		protected void ImageParseXml(System.Xml.XmlReader xmlNode)
		{
			var imgsrc = xmlNode.GetAttribute("src");
			if (!string.IsNullOrEmpty(imgsrc) && System.IO.File.Exists(imgsrc))
			{
				using (var bmp = new System.Drawing.Bitmap(imgsrc))
				{
					bmp.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);
					var l = bmp.Width * bmp.Height * 4;
					var bmpdata = new byte[l];
					for (var x = 0; x < bmp.Width; x++)
					{
						for (var y = 0; y < bmp.Height; y++)
						{
							var pos = y * bmp.Width * 4 + x * 4;
							var pixel = bmp.GetPixel(x, y);
							bmpdata[pos] = pixel.R;
							bmpdata[pos + 1] = pixel.G;
							bmpdata[pos + 2] = pixel.B;
							bmpdata[pos + 3] = pixel.A;
						}
					}
					ByteImage = bmpdata;
					Width = (uint)bmp.Width;
					Height = (uint)bmp.Height;
					Filename = imgsrc;
				}
			}

		}
	}
}
