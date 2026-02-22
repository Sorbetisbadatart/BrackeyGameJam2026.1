using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Experimental.Rendering;

namespace Game.Rendering
{
    public class PixelateFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent passEvent = RenderPassEvent.AfterRendering;
            public Vector2Int targetSize = new Vector2Int(320, 180);
            public bool usePosterize = true;
            public int colorSteps = 16;
            public Shader shader;
        }

        class PixelatePass : ScriptableRenderPass
        {
            static readonly int TargetSizeId = Shader.PropertyToID("_TargetSize");
            static readonly int UsePosterizeId = Shader.PropertyToID("_UsePosterize");
            static readonly int ColorStepsId = Shader.PropertyToID("_ColorSteps");

            Material material;
            Settings settings;
            RTHandle source;
            RTHandle temp;

            public PixelatePass(Material mat, Settings s)
            {
                material = mat;
                settings = s;
            }

            public void Setup(RTHandle src)
            {
                source = src;
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                var desc = renderingData.cameraData.cameraTargetDescriptor;
                desc.msaaSamples = 1;
                desc.depthBufferBits = 0;
                desc.width = Mathf.Max(1, settings.targetSize.x);
                desc.height = Mathf.Max(1, settings.targetSize.y);
                RenderingUtils.ReAllocateHandleIfNeeded(ref temp, desc, FilterMode.Point, TextureWrapMode.Clamp, name: "_PixelateTemp");
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (material == null || source == null) return;
                var cmd = CommandBufferPool.Get("PixelatePost");
                int w = Mathf.Max(1, settings.targetSize.x);
                int h = Mathf.Max(1, settings.targetSize.y);
                cmd.SetGlobalVector(TargetSizeId, new Vector4(w, h, 0, 0));
                cmd.SetGlobalFloat(UsePosterizeId, settings.usePosterize ? 1f : 0f);
                cmd.SetGlobalFloat(ColorStepsId, Mathf.Max(2, settings.colorSteps));
                Blitter.BlitCameraTexture(cmd, source, temp, material, 0);
                Blitter.BlitCameraTexture(cmd, temp, source);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            public override void OnCameraCleanup(CommandBuffer cmd)
            {
            }

            public  void Dispose(bool disposing)
            {
                if (temp != null) temp.Release();
            }

#if UNITY_2022_2_OR_NEWER
            class RGPassData
            {
                public TextureHandle source;
                public TextureHandle destination;
                public Material material;
                public Vector2Int targetSize;
                public bool usePosterize;
                public int colorSteps;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (material == null) return;
                var resources = frameData.Get<UniversalResourceData>();
                var src = resources.activeColorTexture;

                var w = Mathf.Max(1, settings.targetSize.x);
                var h = Mathf.Max(1, settings.targetSize.y);

                var tempDesc = new TextureDesc(w, h)
                {
                    colorFormat = GraphicsFormat.R8G8B8A8_UNorm,
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    name = "_PixelateTempRG"
                };
                var dst = renderGraph.CreateTexture(tempDesc);

                using (var builder = renderGraph.AddRenderPass<RGPassData>("Pixelate Downscale", out var passData))
                {
                    passData.source = src;
                    passData.destination = dst;
                    passData.material = material;
                    passData.targetSize = new Vector2Int(w, h);
                    passData.usePosterize = settings.usePosterize;
                    passData.colorSteps = Mathf.Max(2, settings.colorSteps);

                    builder.ReadTexture(passData.source);
                    builder.WriteTexture(passData.destination);
                    builder.AllowPassCulling(false);
                    builder.SetRenderFunc((RGPassData data, RenderGraphContext ctx) =>
                    {
                        ctx.cmd.SetGlobalVector(TargetSizeId, new Vector4(data.targetSize.x, data.targetSize.y, 0, 0));
                        ctx.cmd.SetGlobalFloat(UsePosterizeId, data.usePosterize ? 1f : 0f);
                        ctx.cmd.SetGlobalFloat(ColorStepsId, data.colorSteps);
                        Blitter.BlitCameraTexture(ctx.cmd, data.source, data.destination, data.material, 0);
                    });
                }

                using (var builder = renderGraph.AddRenderPass<RGPassData>("Pixelate Upscale Composite", out var passData2))
                {
                    passData2.source = dst;
                    passData2.destination = src; // write back to active color
                    passData2.material = null;
                    passData2.targetSize = new Vector2Int(w, h);
                    passData2.usePosterize = settings.usePosterize;
                    passData2.colorSteps = Mathf.Max(2, settings.colorSteps);

                    builder.ReadTexture(passData2.source);
                    builder.WriteTexture(passData2.destination);
                    builder.AllowPassCulling(false);
                    builder.SetRenderFunc((RGPassData data, RenderGraphContext ctx) =>
                    {
                        Blitter.BlitCameraTexture(ctx.cmd, data.source, data.destination);
                    });
                }
            }
#endif
        }

        public Settings settings = new Settings();
        PixelatePass pass;
        Material material;

        public override void Create()
        {
            if (settings.shader == null)
            {
                settings.shader = Shader.Find("Hidden/URP/PixelatePost");
            }
            if (settings.shader != null && material == null)
            {
                material = CoreUtils.CreateEngineMaterial(settings.shader);
            }
            pass = new PixelatePass(material, settings)
            {
                renderPassEvent = settings.passEvent
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (material == null) return;
            pass.Setup(renderer.cameraColorTargetHandle);
            renderer.EnqueuePass(pass);
        }
    }
}
