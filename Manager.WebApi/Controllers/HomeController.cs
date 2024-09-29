using Common.Lib;
using Manager.WebApi.Helper;
using Manager.WebApi.Model;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Manager.WebApi.Helper.WebSocketHelper;
using static System.Net.Mime.MediaTypeNames;

namespace Manager.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class HomeController : ControllerBase
    {
        /// <summary>
        /// 获取 Socket 链接列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ApiResult<List<SocketModel>> GetSocketList()
        {
            try
            {
                return ApiResult.Ok(SocketManager.Instance.GetSocketList());
            }
            catch (Exception ex)
            {
                return ApiResult<List<SocketModel>>.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 发送普通文本信息
        /// </summary>
        /// <param name="conID"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> SendText(string conID, string text)
        {
            try
            {
                var socket = SocketManager.Instance.GetSocketByConId(conID);
                if (socket == null)
                {
                    return ApiResult<bool>.Error("socket 客户端不存在");
                }

                var model = new WsDataModel
                {
                    Action = WsAction.执行端基础信息,
                    DataJson = text
                };

                await WebSocketHelper.Instance.SendTextAsync(socket, JsonSerializer.Serialize(model));
                return ApiResult.Ok(true);
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Error(ex.ToString());
            }
        }


        /// <summary>
        /// 发送文件信息
        /// </summary>
        /// <param name="toDisposeFile"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> SendDLL(string conID, ToDisposeFile disposeFile)
        {
            try
            {
                var socket = SocketManager.Instance.GetSocketByConId(conID);
                if (socket == null)
                {
                    return ApiResult<bool>.Error("socket 客户端不存在");
                }

                await WebSocketHelper.Instance.SendFileJsonAsync(socket, disposeFile);
                return ApiResult.Ok(true);
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Error(ex.ToString());
            }
        }

        [HttpPost]
        public async Task<ApiResult<bool>> LoadRunTask(string conID, string taskName, string cornExp = "default")
        {
            try
            {
                var socket = SocketManager.Instance.GetSocketByConId(conID);
                if (socket == null)
                {
                    return ApiResult<bool>.Error("socket 客户端不存在");
                }

                var taskModel = new LoadRunTaskModel
                {
                    TaskName = taskName,
                    CornExp = cornExp
                };

                var requestModel = new WsDataModel
                {
                    Action = WsAction.加载并执行任务,
                    DataJson = JsonSerializer.Serialize(taskModel)
                };

                await WebSocketHelper.Instance.SendTextAsync(socket, JsonSerializer.Serialize(requestModel));
                return ApiResult.Ok(true);
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Error(ex.ToString());
            }
        }

        [HttpPost]
        public async Task<ApiResult<bool>> PauseTask(string conID, string taskName)
        {
            try
            {
                var socket = SocketManager.Instance.GetSocketByConId(conID);
                if (socket == null)
                {
                    return ApiResult<bool>.Error("socket 客户端不存在");
                }

                var requestModel = new WsDataModel
                {
                    Action = WsAction.任务暂停,
                    DataJson = taskName
                };

                await WebSocketHelper.Instance.SendTextAsync(socket, JsonSerializer.Serialize(requestModel));
                return ApiResult.Ok(true);
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Error(ex.ToString());
            }
        }

        [HttpPost]
        public async Task<ApiResult<bool>> ResumeTask(string conID, string taskName)
        {
            try
            {
                var socket = SocketManager.Instance.GetSocketByConId(conID);
                if (socket == null)
                {
                    return ApiResult<bool>.Error("socket 客户端不存在");
                }

                var requestModel = new WsDataModel
                {
                    Action = WsAction.任务恢复,
                    DataJson = taskName
                };

                await WebSocketHelper.Instance.SendTextAsync(socket, JsonSerializer.Serialize(requestModel));
                return ApiResult.Ok(true);
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Error(ex.ToString());
            }
        }

        [HttpPost]
        public async Task<ApiResult<bool>> RemoveTask(string conID, string taskName)
        {
            try
            {
                var socket = SocketManager.Instance.GetSocketByConId(conID);
                if (socket == null)
                {
                    return ApiResult<bool>.Error("socket 客户端不存在");
                }

                var requestModel = new WsDataModel
                {
                    Action = WsAction.任务删除,
                    DataJson = taskName
                };

                await WebSocketHelper.Instance.SendTextAsync(socket, JsonSerializer.Serialize(requestModel));
                return ApiResult.Ok(true);
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Error(ex.ToString());
            }
        }

        [HttpPost]
        public async Task<ApiResult<bool>> LoadRunTempTask(string conID, string taskName)
        {
            try
            {
                var socket = SocketManager.Instance.GetSocketByConId(conID);
                if (socket == null)
                {
                    return ApiResult<bool>.Error("socket 客户端不存在");
                }

                var taskModel = new LoadRunTaskModel
                {
                    TaskName = taskName,
                    CornExp = "default"
                };

                var requestModel = new WsDataModel
                {
                    Action = WsAction.加载并执行任务,
                    DataJson = JsonSerializer.Serialize(taskModel)
                };

                await WebSocketHelper.Instance.SendTextAsync(socket, JsonSerializer.Serialize(requestModel));
                return ApiResult.Ok(true);
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Error(ex.ToString());
            }
        }

        [HttpPost]
        public async Task<ApiResult<bool>> LoadRunConsumeTask(string conID, string taskName, int count)
        {
            try
            {
                var socket = SocketManager.Instance.GetSocketByConId(conID);
                if (socket == null)
                {
                    return ApiResult<bool>.Error("socket 客户端不存在");
                }

                var taskModel = new ConsumeTaskModel
                {
                    TaskName = taskName,
                    TaskCount = count
                };

                var requestModel = new WsDataModel
                {
                    Action = WsAction.消费者任务调整,
                    DataJson = JsonSerializer.Serialize(taskModel)
                };

                await WebSocketHelper.Instance.SendTextAsync(socket, JsonSerializer.Serialize(requestModel));
                return ApiResult.Ok(true);
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Error(ex.ToString());
            }
        }


    }
}
