using System.Collections.Generic;

#nullable enable

namespace YuoTools.Extend.AI
{
    public partial class StableDiffusionWebuiHelper
    {
        /// <summary>
        /// 基础请求参数
        /// </summary>
        public class StableDiffusionBaseRequest
        {
            public string? prompt { get; set; }
            public string? negative_prompt { get; set; }
            public int? steps { get; set; } = 20;
            public string? sampler_name { get; set; }
            public bool? restore_faces { get; set; }
            public bool? tiling { get; set; }
            public int? n_iter { get; set; } = 1;
            public int? batch_size { get; set; } = 1;
            public float? cfg_scale { get; set; } = 7;
            public long? seed { get; set; } = -1;
            public float? subseed { get; set; } = -1;
            public float? subseed_strength { get; set; }
            public int? seed_resize_from_h { get; set; }
            public int? seed_resize_from_w { get; set; }
            public Dictionary<string, object>? override_settings { get; set; }
            public bool? override_settings_restore_afterwards { get; set; }
            public Dictionary<string, float>? styles { get; set; }
        }

        /// <summary>
        /// 文生图请求参数
        /// </summary>
        public class Text2ImgRequestDto : StableDiffusionBaseRequest
        {
            public bool? enable_hr { get; set; }
            public float? denoising_strength { get; set; }
            public int? firstphase_width { get; set; }
            public int? firstphase_height { get; set; }
            public float? hr_scale { get; set; }
            public string? hr_upscaler { get; set; }
            public int? hr_second_pass_steps { get; set; }
            public int? hr_resize_x { get; set; }
            public int? hr_resize_y { get; set; }
            public int? width { get; set; } = 512;
            public int? height { get; set; } = 512;
        }

        /// <summary>
        /// 图生图请求参数
        /// </summary>
        public class Img2ImgRequestDto : StableDiffusionBaseRequest
        {
            public List<string>? init_images { get; set; }
            public float? denoising_strength { get; set; } = 0.75f;
            public int? image_cfg_scale { get; set; }
            public string? mask { get; set; }
            public int? mask_blur { get; set; }
            public int? inpainting_fill { get; set; }
            public bool? inpaint_full_res { get; set; }
            public int? inpaint_full_res_padding { get; set; }
            public int? inpainting_mask_invert { get; set; }
            public int? resize_mode { get; set; }
            public int? width { get; set; }
            public int? height { get; set; }
        }

        /// <summary>
        /// 通用响应体
        /// </summary>
        public class StableDiffusionResponse
        {
            public List<string> images { get; set; } = new();
            public Dictionary<string, object>? parameters { get; set; }
            public string? info { get; set; }
        }

        /// <summary>
        /// 进度信息
        /// </summary>
        public class ProgressResponse
        {
            public bool active { get; set; }
            public float progress { get; set; }
            public string? eta_relative { get; set; }
            public string? state { get; set; }
            public CurrentImage? current_image { get; set; }
            public string? textinfo { get; set; }
        }

        public class CurrentImage
        {
            public string? text { get; set; }
        }

        /// <summary>
        /// 模型信息
        /// </summary>
        public class SDModelItem
        {
            public string? title { get; set; }
            public string? model_name { get; set; }
            public string? hash { get; set; }
            public string? sha256 { get; set; }
            public string? filename { get; set; }
            public string? config { get; set; }
        }

        /// <summary>
        /// 采样器信息
        /// </summary>
        public class SamplerItem
        {
            public string? name { get; set; }
            public List<string>? aliases { get; set; }
            public List<string>? options { get; set; }
        }

        /// <summary>
        /// 上传图片响应
        /// </summary>
        public class UploadResponse
        {
            public string? name { get; set; }
            public string? data { get; set; }
        }

        /// <summary>
        /// 控制网络(ControlNet)请求参数
        /// </summary>
        public class ControlNetUnitRequest
        {
            public string? input_image { get; set; }
            public string? mask { get; set; }
            public string? module { get; set; }
            public string? model { get; set; }
            public float? weight { get; set; } = 1;
            public float? guidance_start { get; set; } = 0;
            public float? guidance_end { get; set; } = 1;
            public bool? control_mode { get; set; } = false;
            public float? pixel_perfect { get; set; }
            public Dictionary<string, object>? processor_res { get; set; }
            public Dictionary<string, object>? threshold_a { get; set; }
            public Dictionary<string, object>? threshold_b { get; set; }
        }

        /// <summary>
        /// Lora信息
        /// </summary>
        public class LoraInfo
        {
            public string? name { get; set; }
            public string? alias { get; set; }
            public string? path { get; set; }
            public long? metadata { get; set; }
        }

        /// <summary>
        /// 嵌入(Embedding)信息
        /// </summary>
        public class EmbeddingItem
        {
            public int? step { get; set; }
            public int? sd_checkpoint { get; set; }
            public string? sd_checkpoint_name { get; set; }
            public string? shape { get; set; }
            public string? vectors { get; set; }
        }

        /// <summary>
        /// 超网络(Hypernetwork)信息
        /// </summary>
        public class HypernetworkInfo
        {
            public string? name { get; set; }
            public string? path { get; set; }
        }

        /// <summary>
        /// 面部修复器信息
        /// </summary>
        public class FaceRestorerItem
        {
            public string? name { get; set; }
            public string? cmd_dir { get; set; }
        }

        /// <summary>
        /// 重绘模式信息
        /// </summary>
        public class RealesrganItem
        {
            public string? name { get; set; }
            public string? path { get; set; }
            public string? scale { get; set; }
        }

        /// <summary>
        /// 提示词信息
        /// </summary>
        public class PromptStyleItem
        {
            public string? name { get; set; }
            public string? prompt { get; set; }
            public string? negative_prompt { get; set; }
        }

        /// <summary>
        /// 系统选项
        /// </summary>
        public class SDOptions
        {
            public string? sd_model_checkpoint { get; set; }
            public string? sd_vae { get; set; }
            public string? sd_lora { get; set; }
            public bool? sd_hypernetwork { get; set; }
            public Dictionary<string, object>? sd_hypernetwork_strength { get; set; }
            public Dictionary<string, object>? img2img_background_color { get; set; }
            public bool? img2img_color_correction { get; set; }
            public bool? img2img_fix_steps { get; set; }
            public string? img2img_resize_mode { get; set; }
            public Dictionary<string, object>? img2img_mask_blur { get; set; }
            public string? img2img_inpainting_fill { get; set; }
            public bool? img2img_inpaint_full_res { get; set; }
            public Dictionary<string, object>? img2img_inpaint_full_res_padding { get; set; }
            public bool? img2img_inpainting_mask_invert { get; set; }
        }
    }
}