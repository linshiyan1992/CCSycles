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

using System;
using System.Drawing;
using System.Text;
using System.Xml;
using ccl.ShaderNodes.Sockets;
using ccl.Attributes;

namespace ccl.ShaderNodes
{
	/// <summary>
	/// ImageTexture input sockets
	/// </summary>
	public class ImageTextureInputs : Inputs
	{
		/// <summary>
		/// ImageTexture space coordinate to sample texture at
		/// </summary>
		public VectorSocket Vector { get; set; }

		/// <summary>
		/// DecalForward as calculated by the TextureCoordinateNode. Needs to be
		/// connected to work.
		/// </summary>
		public FloatSocket DecalForward { get; set; }
		/// <summary>
		/// DecalInside as calculated by the TextureCoordinateNode. Needs to be
		/// connected to work.
		/// </summary>
		public FloatSocket DecalInside { get; set; }

		internal ImageTextureInputs(ShaderNode parentNode)
		{
			Vector = new VectorSocket(parentNode, "Vector");
			AddSocket(Vector);
			DecalForward = new FloatSocket(parentNode, "DecalForward");
			AddSocket(DecalForward);
			DecalInside = new FloatSocket(parentNode, "DecalInside");
			AddSocket(DecalInside);
		}
	}

	/// <summary>
	/// ImageTexture output sockets
	/// </summary>
	public class ImageTextureOutputs : Outputs
	{
		/// <summary>
		/// ImageTexture Color output
		/// </summary>
		public ColorSocket Color { get; set; }
		/// <summary>
		/// ImageTexture alpha output
		/// </summary>
		public FloatSocket Alpha { get; set; }

		internal ImageTextureOutputs(ShaderNode parentNode)
		{
			Color = new ColorSocket(parentNode, "Color");
			AddSocket(Color);
			Alpha = new FloatSocket(parentNode, "Alpha");
			AddSocket(Alpha);
		}
	}

	[ShaderNode("image_texture")]
	public class ImageTextureNode : TextureNode
	{
		/// <summary>
		/// Image texture input sockets
		/// </summary>
		public ImageTextureInputs ins => (ImageTextureInputs)inputs;

		/// <summary>
		/// Image texture output sockets
		/// </summary>
		public ImageTextureOutputs outs => (ImageTextureOutputs)outputs;

		public ImageTextureNode() : this("an image texture node")
		{
		}

		public ImageTextureNode(string name) :
			base(ShaderNodeType.ImageTexture, name)
		{

			inputs = new ImageTextureInputs(this);
			outputs = new ImageTextureOutputs(this);

			UseAlpha = true;
			AlternateTiles = false;
			ProjectionBlend = 0.0f;
			Interpolation = InterpolationType.Linear;
			ColorSpace = TextureColorSpace.None;
			Projection = TextureProjection.Flat;
			Extension = TextureExtension.Repeat;

			HorizontalSweepStart = 0.0f;
			HorizontalSweepEnd = 1.0f;
			VerticalSweepStart = 0.0f;
			VerticalSweepEnd = 1.0f;
			DecalHeight = 1.0f;
			DecalRadius = 1.0f;
		}

		/// <summary>
		/// ImageTexture texture projection
		/// </summary>
		public TextureProjection Projection { get; set; }
		/// <summary>
		/// ImageTexture texture projection blend
		/// </summary>
		public float ProjectionBlend { get; set; }
		/// <summary>
		/// ImageTexture float image
		/// </summary>
		public bool IsFloat { get; set; }
		/// <summary>
		/// ImageTexture use alpha channel if true
		///
		/// TODO [NATHANLOOK] hook up different alpha usage types. For now auto (true), ignore (false)
		/// </summary>
		public bool UseAlpha { get; set; }
		/// <summary>
		/// Set to true to alternate UV grid tiling. (Rhino specific)
		/// </summary>
		public bool AlternateTiles { get; set; }

		/// <summary>
		/// Give the start for the horizontal sweep of a spherical or cylindrical decal projection
		/// </summary>
		public float HorizontalSweepStart { get; set; }

		/// <summary>
		/// Give the end for the horizontal sweep of a spherical or cylindrical decal projection
		/// </summary>
		public float HorizontalSweepEnd { get; set; }

		/// <summary>
		/// Give the start for the vertical sweep of a spherical or cylindrical decal projection
		/// </summary>
		public float VerticalSweepStart { get; set; }

		/// <summary>
		/// Give the end for the vertical sweep of a spherical or cylindrical decal projection
		/// </summary>
		public float VerticalSweepEnd { get; set; }

		/// <summary>
		/// Height for cylindrical projection of a decal
		/// </summary>
		public float DecalHeight { get; set; }

		/// <summary>
		/// Radius for cylindrical or spherical decal projection
		/// </summary>
		public float DecalRadius { get; set; }

		/// <summary>
		/// Direction of the decal: forward, backward
		/// </summary>
		public DecalDirection Direction { get; set; }

		internal override void SetEnums(uint clientId, uint shaderId)
		{
			CSycles.shadernode_set_enum(clientId, shaderId, Id, Type, "color_space", (int)ColorSpace);
			CSycles.shadernode_set_enum(clientId, shaderId, Id, Type, "projection", (int)Projection);
			CSycles.shadernode_set_enum(clientId, shaderId, Id, Type, "interpolation", (int)Interpolation);
			CSycles.shadernode_set_enum(clientId, shaderId, Id, Type, "decal_projection", (int)Direction);
		}

		internal override void SetDirectMembers(uint clientId, uint sceneId, uint shaderId)
		{
			base.SetDirectMembers(clientId, sceneId, shaderId);
			CSycles.shadernode_set_member_float(clientId, shaderId, Id, Type, "projection_blend", ProjectionBlend);
			CSycles.shadernode_set_member_float(clientId, shaderId, Id, Type, "decal_height", DecalHeight);
			CSycles.shadernode_set_member_float(clientId, shaderId, Id, Type, "decal_radius", DecalRadius);
			CSycles.shadernode_set_member_float(clientId, shaderId, Id, Type, "decal_hor_start", HorizontalSweepStart);
			CSycles.shadernode_set_member_float(clientId, shaderId, Id, Type, "decal_hor_end", HorizontalSweepEnd);
			CSycles.shadernode_set_member_float(clientId, shaderId, Id, Type, "decal_ver_start", VerticalSweepStart);
			CSycles.shadernode_set_member_float(clientId, shaderId, Id, Type, "decal_ver_end", VerticalSweepEnd);
			CSycles.shadernode_set_member_int(clientId, shaderId, Id, Type, "extension", (int)Extension);
			CSycles.shadernode_set_member_bool(clientId, shaderId, Id, Type, "use_alpha", UseAlpha);
			CSycles.shadernode_set_member_bool(clientId, shaderId, Id, Type, "is_linear", IsLinear);
			CSycles.shadernode_set_member_bool(clientId, shaderId, Id, Type, "alternate_tiles", AlternateTiles);
			if (FloatImagePtr != IntPtr.Zero)
			{
				CSycles.shadernode_set_member_float_img(clientId, sceneId, shaderId, Id, Type, "builtin-data", Filename ?? String.Format("{0}-{0}-{0}", clientId, shaderId, Id), FloatImagePtr, Width, Height, 1, 4);
			}
			else if (ByteImagePtr != IntPtr.Zero)
			{
				CSycles.shadernode_set_member_byte_img(clientId, sceneId, shaderId, Id, Type, "builtin-data", Filename ?? String.Format("{0}-{0}-{0}", clientId, shaderId, Id), ByteImagePtr, Width, Height, 1, 4);
			}
		}
		private void SetProjection(string projection)
		{
			projection = projection.Replace(" ", "_");
			Projection = (TextureProjection)Enum.Parse(typeof(TextureProjection), projection, true);
		}

		internal override void ParseXml(XmlReader xmlNode)
		{
			var cs = xmlNode.GetAttribute("color_space");
			if (!string.IsNullOrEmpty(cs))
			{
				SetColorSpace(cs);
			}
			var projection = xmlNode.GetAttribute("projection");
			if (!string.IsNullOrEmpty(projection))
			{
				SetProjection(projection);
			}
			var extension = xmlNode.GetAttribute("extension");
			if (!string.IsNullOrEmpty(extension))
			{
				SetExtension(extension);
			}
			var interpolation = xmlNode.GetAttribute("interpolation");
			if (!string.IsNullOrEmpty(interpolation))
			{
				SetInterpolation(interpolation);
			}
			ImageParseXml(xmlNode);
		}

		public override string CreateXmlAttributes()
		{
			var code = new StringBuilder($" projection=\"{Projection}\" ", 1024);
			code.Append($" color_space=\"{ColorSpace}\"");
			code.Append($" extension=\"{Extension}\"");
			code.Append($" interpolation=\"{Interpolation}\"");
			code.Append($" use_alpha=\"{UseAlpha}\"");
			code.Append($" is_linear=\"{IsLinear}\"");
			code.Append($" alternate_tiles=\"{AlternateTiles}\"");
			if (Filename != null)
			{
				code.Append($" src=\"{Filename.Replace("\\", "\\\\")}\"");
			}

			return code.ToString();
		}

		public override string CreateCodeAttributes()
		{
			var code = new StringBuilder($"{VariableName}.Projection = {Projection};", 1024);
			code.Append($"{VariableName}.ColorSpace = {ColorSpace};");
			code.Append($"{VariableName}.Extension = {Extension};");
			code.Append($"{VariableName}.Interpolation = {Interpolation};");
			code.Append($"{VariableName}.UseAlpha = {UseAlpha};");
			code.Append($"{VariableName}.IsLinear = {IsLinear};");
			code.Append($"{VariableName}.AlternateTiles = {AlternateTiles};");

			return code.ToString();
		}
	}
}
