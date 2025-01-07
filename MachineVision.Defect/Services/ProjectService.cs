using MachineVision.Defect.Models;
using MachineVision.Shared.Services;
using MachineVision.Shared.Services.Tables;
using Newtonsoft.Json;

namespace MachineVision.Defect.Services
{
    public class ProjectService : BaseService
    {
        private readonly IAppMapper mapper;

        public ProjectService(IAppMapper mapper)
        {
            this.mapper = mapper;
        }


        #region 项目管理

        public async Task CreateOrUpdateAsync(ProjectModel input)
        {
            var model = mapper.Map<Project>(input);
            if (input.Id > 0)
            {
                var result = await Sqlite.Select<Project>().Where(t => t.Id == model.Id).FirstAsync();
                if (result != null)
                {
                    model.ReferParameter = JsonConvert.SerializeObject(input.ReferSetting);
                    model.UpdateDate = DateTime.Now;
                    await Sqlite.Update<Project>()
                          .SetDto(model)
                          .Where(q => q.Id == input.Id)
                          .ExecuteAffrowsAsync();
                }
            }
            else
            {
                var result = await Sqlite.Select<Project>().FirstAsync(q => q.Name.Equals(input.Name));
                if (!result)
                {
                    model.CreateDate = DateTime.Now;
                    model.UpdateDate = DateTime.Now;
                    var row = await Sqlite.Insert(model).ExecuteAffrowsAsync();
                }
            }
        }

        public async Task DeleteAsync(int Id)
        {
            await Sqlite.Delete<Project>().Where(a => a.Id == Id).ExecuteAffrowsAsync();
        }

        public async Task<List<ProjectModel>> GetListAsync(string filterText)
        {
            var models = await Sqlite.Select<Project>()
                .Where(q => q.Name.Contains(filterText))
                .ToListAsync();
            return mapper.Map<List<ProjectModel>>(models);
        }

        public async Task<ProjectModel> GetProjectByIdAsync(int Id)
        {
            var result = await Sqlite.Select<Project>().Where(t => t.Id == Id).FirstAsync();

            if (result != null)
                return mapper.Map<ProjectModel>(result);

            return null;
        }

        #endregion

        #region 检测区域管理

        /// <summary>
        /// 根据项目ID查询检测列表
        /// </summary>
        /// <param name="ProjectId">项目ID</param>
        /// <returns></returns>
        public async Task<List<InspecRegionModel>> GetRegionListAsync(int ProjectId)
        {
            var list = await Sqlite.Select<InspecRegion>().
                Where(q => q.ProjectId == ProjectId).ToListAsync();

            return mapper.Map<List<InspecRegionModel>>(list);
        }

        /// <summary>
        /// 创建检测区域
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task CreateRegionAsync(InspecRegionModel input)
        {
            var model = mapper.Map<InspecRegion>(input);
            await Sqlite.Insert(model).ExecuteAffrowsAsync();
        }

        /// <summary>
        /// 更新检测区域
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task UpdateRegionAsync(InspecRegionModel input)
        {
            var model = mapper.Map<InspecRegion>(input);

            //更新检测区域的参数设置
            model.Parameter = input.Context.GetJsonParameter();
            model.MatchParameter = JsonConvert.SerializeObject(input.MatchSetting);

            var row = await Sqlite.Update<InspecRegion>()
                 .SetDto(model)
                 .Where(q => q.Id == model.Id).ExecuteAffrowsAsync();
        }

        /// <summary>
        /// 删除检测区域
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task DeleteRegionAsync(int Id)
        {
            await Sqlite.Delete<InspecRegion>().Where(q => q.Id == Id).ExecuteAffrowsAsync();
        }

        #endregion
    }
}
