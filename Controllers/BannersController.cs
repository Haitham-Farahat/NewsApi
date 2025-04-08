using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewsApi.AppHandler.Genrics.Intrefaces;
using NewsApi.AppHandler.Wrapper.WorkWrapper;
using NewsAPI.Domain.AppEntity;
using NewsApi.AppHandler.Wrapper.WorkWrapper;
using NewsAPI.Domain.DTOS.UserAccountDto;
using System.Reflection;
using NewsApi.AppHandler.Mapping;
using NewsAPI.Domain.DTOS.BannersDto;
namespace NewsAPIApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BannersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public BannersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BannersReadDTO>>> GetBanners()
        {
            try
            {
                var banners = await _unitOfWork.Repository<Banners>().GetAllWithAsync();
                var ResultBanners = BannerMapProfile.ToPlatformReadDTOList(banners);
                return Ok(ResultResponse<IEnumerable<BannersReadDTO>>.Success(ResultBanners, "sucess"));
            }
            catch (Exception ex)
            {
                return Ok(ResultResponse<Banners>.Fail(ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BannersReadDTO>> GetBanner(int id)
        {
            try
            {
                var banner = await _unitOfWork.Repository<Banners>().GetbyIdAsync(id);

                if (banner == null)
                {
                    return Ok(ResultResponse<Banners>.Fail("Banner not found"));
                }
                var ResultBanners = BannerMapProfile.MapToBannersTOformReadDTO(banner);

                return Ok(ResultResponse<BannersReadDTO>.Success(ResultBanners, "success"));
            }
            catch (Exception ex)
            {
                return Ok(ResultResponse<BannersReadDTO>.Fail(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutBanner(int id, Banners banner)
        {
            try
            {
                if (id != banner.Id)
                {
                    return Ok(ResultResponse<Banners>.Fail("ID mismatch between route and object"));
                }

                // Get the existing entity from database first
                var existingBanner = await _unitOfWork.Repository<Banners>().GetbyIdAsync(id);

                if (existingBanner == null)
                {
                    return Ok(ResultResponse<Banners>.Fail($"Banner with ID {id} not found"));
                }

                // Detach the existing entity from the context
                _unitOfWork.Detach(existingBanner);

                // Now update with the new entity
                _unitOfWork.Repository<Banners>().Update(banner);
                await _unitOfWork.CompleteAsync();

                return Ok(ResultResponse<Banners>.Success(banner, "Banner updated successfully"));
            }
            catch (Exception ex)
            {
                return Ok(ResultResponse<Banners>.Fail(ex.Message));
            }
        }

        // POST: api/Banners
        [HttpPost]
        public async Task<ActionResult<BannersReadDTO>> PostBanner(BannersCreateDto banner)
        {
            try
            {
                var mapping = BannerMapProfile.MapToBannerCreateDtosTOBanners(banner);
                if (mapping == null)
                {
                    return Ok(ResultResponse<Banners>.Fail("empty data send"));
                }
                await _unitOfWork.Repository<Banners>().AddAsync(mapping);
                await _unitOfWork.CompleteAsync();

                // Convert the saved Banners object to a BannersReadDTO
                var resultMapping = BannerMapProfile.MapToBannersTOformReadDTO(mapping);

                return Ok(ResultResponse<BannersReadDTO>.Success(resultMapping, "Banner created successfully"));
            }
            catch (Exception ex)
            {
                return Ok(ResultResponse<Banners>.Fail(ex.Message));
            }
        }

        // DELETE: api/Banners/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            try
            {
                var banner = await _unitOfWork.Repository<Banners>().GetbyIdAsync(id);

                if (banner == null)
                {
                    return Ok(ResultResponse<Banners>.Fail($"Banner with ID {id} not found"));
                }

                _unitOfWork.Repository<Banners>().DeleteAsync(banner);
                await _unitOfWork.CompleteAsync();

                return Ok(ResultResponse<object>.Success(null, "Banner deleted successfully"));
            }
            catch (Exception ex)
            {
                return Ok(ResultResponse<object>.Fail(ex.Message));
            }
        }
    }
}