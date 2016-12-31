using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.IO;

// TODO:
// . configurable shaders
// . fix optical flow when image resolution is different from screen resolution
// . support custom color wheels in optical flow via lookup textures
// . tests
// . better example scene(s)

[RequireComponent (typeof(Camera))]
public class ImageSynthesis : MonoBehaviour {
	
	static readonly string[] PassNames = { "_img", "_id", "_layer", "_depth", "_flow" };
	private Camera[] captureCameras = new Camera[PassNames.Length];

	void Start()
	{
		for (int q = 0; q < captureCameras.Length; q++)
			captureCameras[q] = CreateHiddenCamera (PassNames[q], q);

		captureCameras[0].enabled = false;	

		var shader = Shader.Find("Hidden/UniformColor");
		SetupCameraWithReplacementShaderAndBlackBackground(captureCameras[1], shader, 0);
		SetupCameraWithReplacementShaderAndBlackBackground(captureCameras[2], shader, 1);

		var shader2 = Shader.Find("Hidden/LinearDepth");
		var shader3 = Shader.Find("Hidden/OpticalFlow");
		SetupCameraWithPostShader(captureCameras[3], shader2, DepthTextureMode.Depth);
		SetupCameraWithPostShader(captureCameras[4], shader3, DepthTextureMode.Depth | DepthTextureMode.MotionVectors);

		OnSceneChange();
	}
	
	private Camera CreateHiddenCamera(string name, int targetDisplay)
	{
		var go = new GameObject (name, typeof (Camera));
		go.hideFlags = HideFlags.HideAndDontSave;;
		go.transform.parent = transform;

		var newCamera = go.GetComponent<Camera>();
		newCamera.CopyFrom(GetComponent<Camera>());
		newCamera.targetDisplay = targetDisplay;

		return newCamera;
	}

	static private void SetupCameraWithReplacementShaderAndBlackBackground(Camera cam, Shader shader, int source)
	{
		var cb = new CommandBuffer();
		cb.SetGlobalFloat("_Source", source); // @TODO: CommandBuffer is missing SetGlobalInt() method
		cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, cb);
		cam.AddCommandBuffer(CameraEvent.BeforeFinalPass, cb);
		cam.SetReplacementShader(shader, "");
		cam.backgroundColor = Color.black;
		cam.clearFlags = CameraClearFlags.SolidColor;
	}

	static private void SetupCameraWithPostShader(Camera cam, Shader shader, DepthTextureMode depthTextureMode)
	{
		var cb = new CommandBuffer();
		cb.Blit(null, BuiltinRenderTextureType.CurrentActive, new Material(shader));
		cam.AddCommandBuffer(CameraEvent.AfterEverything, cb);
		cam.depthTextureMode = depthTextureMode;
	}

	public void OnSceneChange()
	{
		var renderers = Object.FindObjectsOfType<Renderer>();
		var mpb = new MaterialPropertyBlock();
		foreach (var r in renderers)
		{
			var id = r.gameObject.GetInstanceID();
			var layer = r.gameObject.layer;
			var tag = r.gameObject.tag;

			mpb.SetColor("_ObjectColor", ColorEncoding.EncodeIDAsColor(id));
			mpb.SetColor("_ClusterColor", ColorEncoding.EncodeLayerAsColor(layer));
			r.SetPropertyBlock(mpb);
		}
	}

	public void Save(string filename, int width = -1, int height = -1, string path = "")
	{
		if (width <= 0 || height <= 0)
		{
			width = Screen.width;
			height = Screen.height;
		}

		var filenameExtension = System.IO.Path.GetExtension(filename);
		if (filenameExtension == "")
			filenameExtension = ".png";
		var filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

		var pathWithoutExtension = Path.Combine(path, filenameWithoutExtension);

		Save(pathWithoutExtension, filenameExtension, width, height);
	}

	private void Save(string filenameWithoutExtension, string filenameExtension, int width, int height)
	{
		var rt = RenderTexture.GetTemporary(width, height, 24);
		var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

		var prevActiveRT = RenderTexture.active;
		RenderTexture.active = rt;

		foreach (var cam in captureCameras)
		{
			// Render to offscreen texture (readonly from CPU side)
			cam.targetTexture = rt;
			cam.Render();

			// Read offsreen texture contents into the CPU readable texture
			tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			tex.Apply();

			// Encode texture into PNG
			var bytes = tex.EncodeToPNG();
			File.WriteAllBytes(filenameWithoutExtension + cam.name + filenameExtension, bytes);					

			cam.targetTexture = null;
		}

		Object.Destroy(tex);

		RenderTexture.active = prevActiveRT;
		RenderTexture.ReleaseTemporary(rt);
	}
}