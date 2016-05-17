﻿namespace FinerGames {

	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	public class DeformableSprite : DeformableMesh {

		public Sprite[] sprites;
		public Sprite sprite;

		public Texture2D texture;

		public int selectedSprite;

		public List<Transform> Vertices = new List<Transform> ();
		public List<int> Triangles = new List<int> ();

		public void SetTextureAndSprites(Texture2D sourceTexture, Sprite[] textureSprites) {
			texture = sourceTexture;
			sprites = textureSprites;
			if(selectedSprite < 0 || (sprites != null && selectedSprite >= sprites.Length))
				selectedSprite = 0;

			SelectSprite(selectedSprite);
		}

		public void SelectSprite(int index)
		{
			if(sprites == null || sprites.Length == 0 || texture == null)
				return;

			if(index < 0 || index > sprites.Length)
				index = 0;

			selectedSprite = index;

			Sprite sprite = sprites[index];

			Vector2 length = new Vector2(texture.width, texture.height);

			ApplyNewOffset(-new Vector2((sprite.rect.x + sprite.rect.width * 0.5f - sprite.texture.width * 0.5f) / length.x, (sprite.rect.y + sprite.rect.height * 0.5f - sprite.texture.height * 0.5f) / length.y));
		}

		public override void Initialize(bool forceUpdate = false) {
			base.Initialize(forceUpdate);

			meshLinkType = MeshLinkType.SpriteMeshLink;

			if(sprites == null || sprites.Length == 0 || texture == null)
			{
				Debug.LogWarning("No sprites and/or texture found. exiting operation.");
				return;
			}

			if(LinkedMeshRenderer.sharedMaterial == null)
				LinkedMeshRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));

			if(LinkedMeshRenderer.sharedMaterial.mainTexture == null)
				LinkedMeshRenderer.sharedMaterial.mainTexture = texture;


			if(selectedSprite < 0 || selectedSprite >= sprites.Length)
				selectedSprite = 0;

			Sprite sprite = sprites[selectedSprite];

			if(sprite == null)
				return;

			Vertices.Clear ();
			Vertices.Add (this.transform);
			var pointMasses = this.GetComponentsInChildren<PointMass> ();
			for (int i = 0; i < pointMasses.Length; i++) {
				Vertices.Add (pointMasses [i].transform);
			}

			//			if(forceUpdate || LinkedMeshFilter.sharedMesh.vertexCount != body.Shape.VertexCount)
			if(forceUpdate || LinkedMeshFilter.sharedMesh.vertexCount != Vertices.Count) {

				//				if(LinkedMeshFilter.sharedMesh.vertexCount != body.Shape.VertexCount)
				if(LinkedMeshFilter.sharedMesh.vertexCount != Vertices.Count)
					LinkedMeshFilter.sharedMesh.Clear();

				////				vertices = new Vector3[body.Shape.VertexCount];
				//				vertices = new Vector3[Vertices.Count];
				////				for(int i = 0; i < vertices.Length; i++)
				//				vertices[0] = (Vector3)body.transform.localPosition;
				//				for(int i = 1; i < vertices.Length; i++)
				//					vertices[i] = (Vector3)body.PointMasses[i - 1].transform.position;

				Vector2 length = new Vector2(sprite.texture.width, sprite.texture.height);
				float pixelsToUnits = sprite.textureRect.width / sprite.bounds.size.x;
				length /= pixelsToUnits;

				Vector2[] uvPts = new Vector2[Vertices.Count];

				//				var origin = new Vector2 (this.transform.position.x - (sprite.textureRect.width / .5f), this.transform.position.y - (sprite.textureRect.width / .5f));

				var minX = Vertices.Min (_ => _.position.x);
				var maxX = Vertices.Max (_ => _.position.x);
				var minY = Vertices.Min (_ => _.position.y);
				var maxY = Vertices.Max (_ => _.position.y);

				for(int i = 0; i < uvPts.Length; i++)
				{
					//					if(i == 0)
					//						uvPts[i] = Vector2.zero - pivotOffset;
					//					else
					//						uvPts[i] = (Vector2)Vertices[i].position - pivotOffset;
					//					
					////					uvPts[i] = (Vector2)Vertices[i].position - pivotOffset;
					//					uvPts[i] = VectorTools.rotateVector(uvPts[i], angle);
					//					uvPts[i] = new Vector2(uvPts[i].x / scale.x, uvPts[i].y / scale.y);
					//					uvPts[i] = new Vector2(0.5f + uvPts[i].x / length.x, 0.5f + uvPts[i].y / length.y);

					var x = (Vertices [i].position.x - minX) / (maxX - minX);
					var y = (Vertices [i].position.y - minY) / (maxY - minY);

					uvPts [i] = new Vector2 (x, y);
					//					uvPts[i] -= offset;

				}

				Triangles.Clear ();
				// the first vertice is the center, so doesn't need to be done
				for (int i = 1; i < Vertices.Count; i++) {
					Triangles.Add (i);

					// if on the last vertice, wrap around
					if(i == Vertices.Count - 1)
						Triangles.Add (1);
					else
						Triangles.Add (i + 1);

					Triangles.Add (0);
				}

				var positions = new Vector3[Vertices.Count];
				for (int i = 0; i < Vertices.Count; i++) {
					if(i == 0)
						positions [i] = Vector3.zero;
					else
						positions [i] = Vertices [i].localPosition;
				}
				LinkedMeshFilter.sharedMesh.vertices = positions;
				LinkedMeshFilter.sharedMesh.uv = uvPts;
				//				LinkedMeshFilter.sharedMesh.triangles = body.Shape.Triangles;
				LinkedMeshFilter.sharedMesh.triangles = Triangles.ToArray ();
				LinkedMeshFilter.sharedMesh.colors = null;
				if(CalculateNormals)
					LinkedMeshFilter.sharedMesh.RecalculateNormals();
				if(CalculateTangents)
					calculateMeshTangents();

				LinkedMeshFilter.sharedMesh.RecalculateBounds();
				LinkedMeshFilter.sharedMesh.Optimize();
				LinkedMeshFilter.sharedMesh.MarkDynamic();
			}
		}

		public void UpdateMesh () {

			//			if(vertices.Length != Vertices.Count)
			//				vertices = new Vector3[Vertices.Count];
			//
			//			for(int i = 0; i < vertices.Length; i++)
			//				vertices[i] = Vertices[i];

			var positions = new Vector3[Vertices.Count];
			for (int i = 0; i < Vertices.Count; i++) {
				if(i == 0)
					positions [i] = Vector3.zero;
				else
					positions [i] = Vertices [i].localPosition;
			}
			LinkedMeshFilter.sharedMesh.vertices = positions;
			LinkedMeshFilter.sharedMesh.RecalculateBounds ();
		}

		/// <summary>
		/// Update the pivot point.
		/// </summary>
		/// <param name="change">The amount by which to change the pivot point.</param>
		/// <param name="monoBehavior">The MonoBehavior that may have been affected by change in pivot point. This is used mainly for setting it dirty in the Editor.</param>
		/// <returns>Whether the pivot point was updated.</returns>
		public override bool UpdatePivotPoint (Vector2 change, out MonoBehaviour monoBehavior)
		{
			if(LinkedMeshFilter.sharedMesh == null)
				Initialize (true);

			pivotOffset -= change; //TODO update this in all mesh links!!!!

			var positions = new Vector3[Vertices.Count];
			for (int i = 0; i < Vertices.Count; i++) {
				if(i == 0)
					positions [i] = Vector3.zero;
				else
					positions [i] = Vertices [i].localPosition;
			}

			for(int i = 0; i < LinkedMeshFilter.sharedMesh.vertices.Length; i++)
				positions [i] -= (Vector3)change;

			LinkedMeshFilter.sharedMesh.vertices = positions;

			//			vertices = LinkedMeshFilter.sharedMesh.vertices;
			//			for(int i = 0; i < LinkedMeshFilter.sharedMesh.vertices.Length; i++)
			//				vertices[i] -= (Vector3)change;
			//
			//			LinkedMeshFilter.sharedMesh.vertices = vertices;

			return base.UpdatePivotPoint (change, out monoBehavior);
		}
	}
}
