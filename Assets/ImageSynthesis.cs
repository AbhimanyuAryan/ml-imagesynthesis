using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.IO;

// @TODO:
// . support custom color wheels in optical flow via lookup textures
// . support custom depth encoding
// . support multiple overlay cameras
// . tests
// . better example scene(s)

// @KNOWN ISSUES
// . Motion Vectors can produce incorrect results in Unity 5.5.f3 when
// 	  rendering several cameras with different aspect ratios - vectors stretch to the sides of the screen
// . Depth is not anti-aliased atlhough the main image is.

[RequireComponent (typeof(Camera))]
public class ImageSynthesis : MonoBehaviour {
	
	static readonly string[] PassNames = { "_img", "_id", "_layer", "_depth", "_flow" };
	private Camera[] captureCameras = new Camera[PassNames.Length - 1];

	public Shader colorPassShader;
	public Shader depthPassShader;
	public Shader opticalFlowPassShader;

	public float opticalFlowSensitivity = 1.0f;

	// cached materials
	private Material depthPassMaterial;
	private Material opticalFlowPassMaterial;

	void Start()
	{
		// default fallbacks, if shaders are unspecified
		if (!colorPassShader)
			colorPassShader = Shader.Find("Hidden/UniformColor");
		if (!depthPassShader)
			depthPassShader = Shader.Find("Hidden/CompressedDepth");
		if (!opticalFlowPassShader)
			opticalFlowPassShader = Shader.Find("Hidden/OpticalFlow");

		for (int q = 0; q < captureCameras.Length; q++)
			captureCameras[q] = CreateHiddenCamera (PassNames[q + 1]);

		OnCameraChange();
		OnSceneChange();
	}

	void LateUpdate()
	{
		OnCameraChange();
	}
	
	private Camera CreateHiddenCamera(string name)
	{
		var go = new GameObject (name, typeof (Camera));
		go.hideFlags = HideFlags.HideAndDontSave;
		go.transform.parent = transform;

		var newCamera = go.GetComponent<Camera>();
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

	static private void SetupCameraWithPostShader(Camera cam, Material material, DepthTextureMode depthTextureMode)
	{
		var cb = new CommandBuffer();
		cb.Blit(null, BuiltinRenderTextureType.CurrentActive, material);
		cam.AddCommandBuffer(CameraEvent.AfterEverything, cb);
		cam.depthTextureMode = depthTextureMode;
	}

	public void OnCameraChange()
	{
		int targetDisplay = 1;
		foreach (var cam in captureCameras)
		{
			// cleanup capturing camera
			cam.RemoveAllCommandBuffers();

			// copy all "main" camera parameters into capturing camera
			cam.CopyFrom(GetComponent<Camera>());

			// set targetDisplay here since it gets overriden by CopyFrom()
			cam.targetDisplay = targetDisplay++;
		}

		// cache materials and setup material properties
		if (!depthPassMaterial || depthPassMaterial.shader != depthPassShader)
			depthPassMaterial = new Material(depthPassShader);

		if (!opticalFlowPassMaterial || opticalFlowPassMaterial.shader != opticalFlowPassShader)
			opticalFlowPassMaterial = new Material(opticalFlowPassShader);
		opticalFlowPassMaterial.SetFloat("_Sensitivity", opticalFlowSensitivity);

		// setup command buffers and replacement shaders
		SetupCameraWithReplacementShaderAndBlackBackground(captureCameras[0], colorPassShader, 0);
		SetupCameraWithReplacementShaderAndBlackBackground(captureCameras[1], colorPassShader, 1);

		SetupCameraWithPostShader(captureCameras[2], depthPassMaterial, DepthTextureMode.Depth);
		SetupCameraWithPostShader(captureCameras[3], opticalFlowPassMaterial, DepthTextureMode.Depth | DepthTextureMode.MotionVectors);
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

		// execute as coroutine to wait for the EndOfFrame before starting capture
		StartCoroutine(
			WaitForEndOfFrameAndSave(pathWithoutExtension, filenameExtension, width, height));
	}

	private IEnumerator WaitForEndOfFrameAndSave(string filenameWithoutExtension, string filenameExtension, int width, int height)
	{
		yield return new WaitForEndOfFrame();
		Save(filenameWithoutExtension, filenameExtension, width, height);
	}

	private void Save(string filenameWithoutExtension, string filenameExtension, int width, int height)
	{
		var mainCamera = GetComponent<Camera>();
		Save(mainCamera, filenameWithoutExtension + PassNames[0] + filenameExtension, width, height);

		foreach (var cam in captureCameras)
			Save(cam, filenameWithoutExtension + cam.name + filenameExtension, width, height);
	}

	private void Save(Camera cam, string filename, int width, int height)
	{
		var mainCamera = GetComponent<Camera>();
		var renderRT = RenderTexture.GetTemporary(mainCamera.pixelWidth, mainCamera.pixelHeight, 24);
	
		var saveRT = RenderTexture.GetTemporary(width, height, 24);
		var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

		var prevActiveRT = RenderTexture.active;
		var prevCameraRT = cam.targetTexture;

		// render to offscreen texture (readonly from CPU side)
		RenderTexture.active = renderRT;
		cam.targetTexture = renderRT;
		cam.Render();

		// blit to rescale (see issue with Motion Vectors in @KNOWN ISSUES)
		RenderTexture.active = saveRT;
		Graphics.Blit(renderRT, saveRT);

		// read offsreen texture contents into the CPU readable texture
		tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
		tex.Apply();

		// encode texture into PNG
		var bytes = tex.EncodeToPNG();
		File.WriteAllBytes(filename, bytes);					

		// restore state and cleanup
		cam.targetTexture = prevCameraRT;
		RenderTexture.active = prevActiveRT;

		Object.Destroy(tex);
		RenderTexture.ReleaseTemporary(renderRT);
		RenderTexture.ReleaseTemporary(saveRT);
	}
}
