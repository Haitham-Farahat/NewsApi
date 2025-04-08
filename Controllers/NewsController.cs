using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewsApi.AppHandler.Genrics.Intrefaces;
using NewsApi.AppHandler.Wrapper.WorkWrapper;
using NewsAPI.Domain.AppEntity;

namespace NewsAPIApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public NewsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<News>>> GetNews()
        {
            try
            {
                var News = await _unitOfWork.Repository<News>().GetAllWithAsync();
                return Ok(ResultResponse<IEnumerable<News>>.Success(News, "sucess"));
            }
            catch (Exception ex)
            {
                return Ok(ResultResponse<News>.Fail(ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<News>> GetNews(int id)
        {
            try
            {
                var News = await _unitOfWork.Repository<News>().GetbyIdAsync(id);

                if (News == null)
                {
                    return Ok(ResultResponse<News>.Fail("News not found"));
                }

                return Ok(ResultResponse<News>.Success(News, "success"));
            }
            catch (Exception ex)
            {
                return Ok(ResultResponse<News>.Fail(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutNews(int id, News News)
        {
            try
            {
                if (id != News.Id)
                {
                    return Ok(ResultResponse<News>.Fail("ID mismatch between route and object"));
                }

                // Get the existing entity from database first
                var existingNews = await _unitOfWork.Repository<News>().GetbyIdAsync(id);

                if (existingNews == null)
                {
                    return Ok(ResultResponse<News>.Fail($"News with ID {id} not found"));
                }

                // Detach the existing entity from the context
                _unitOfWork.Detach(existingNews);

                // Now update with the new entity
                _unitOfWork.Repository<News>().Update(News);
                await _unitOfWork.CompleteAsync();

                return Ok(ResultResponse<News>.Success(News, "News updated successfully"));
            }
            catch (Exception ex)
            {
                return Ok(ResultResponse<News>.Fail(ex.Message));
            }
        }

        // POST: api/News
        [HttpPost]
        public async Task<ActionResult<News>> PostNews(News News)
        {
            try
            {
                await _unitOfWork.Repository<News>().AddAsync(News);
                await _unitOfWork.CompleteAsync();

                return Ok(ResultResponse<News>.Success(News, "News created successfully"));
            }
            catch (Exception ex)
            {
                return Ok(ResultResponse<News>.Fail(ex.Message));
            }
        }

        // DELETE: api/News/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNews(int id)
        {
            try
            {
                var News = await _unitOfWork.Repository<News>().GetbyIdAsync(id);

                if (News == null)
                {
                    return Ok(ResultResponse<News>.Fail($"News with ID {id} not found"));
                }

                _unitOfWork.Repository<News>().DeleteAsync(News);
                await _unitOfWork.CompleteAsync();

                return Ok(ResultResponse<object>.Success(null, "News deleted successfully"));
            }
            catch (Exception ex)
            {
                return Ok(ResultResponse<object>.Fail(ex.Message));
            }
        }
    }
}