using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace WarGame_True.Infrastructure.Auth {
    /// <summary>
    /// 封装 unity - authentication 包
    /// </summary>
    public class WarAuthService {

        /// <summary>
        /// 生成一个 InitializationOptions 类
        /// </summary>
        public InitializationOptions GenerateAuthenticationOptions(string profile) {
            try {
                //Initialization options act as a key-value store
                //that can facilitate unique SDK initialization specifications.
                var unityAuthenticationInitOptions = new InitializationOptions();
                if (profile.Length > 0) {
                    unityAuthenticationInitOptions.SetProfile(profile);
                }

                return unityAuthenticationInitOptions;
            } catch (Exception e) {
                var reason = $"{e.Message} ({e.InnerException?.Message})";
                throw;
            }
        }

        /// <summary>
        /// 初始化Unity Gaming Service
        /// </summary>
        /// <param name="options"></param>
        public async void InitAuthService(InitializationOptions options) {
            try {
                await UnityServices.InitializeAsync(options);

                if (!AuthenticationService.Instance.IsSignedIn) {
                    // 未登录 则匿名登录
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                Debug.Log("初始化UGS完成！");
            } catch (Exception e) {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// 更换玩家的信息（Profile）
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public async Task SwitchProfileAndResignInAsync(string profile) {
            if (AuthenticationService.Instance.IsSignedIn) {
                // 必须要注销后 才可以更换个人信息
                AuthenticationService.Instance.SignOut();
            }

            // 
            AuthenticationService.Instance.SwitchProfile(profile);

            try {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }catch (Exception e) {
                var reason = $"{e.Message} ({e.InnerException?.Message})";
                throw;
            }


        }

        public async Task<bool> EnsurePlayerIsAuthorized() {
            if (AuthenticationService.Instance.IsAuthorized) {
                // 确认已经登录
                return true;
            }

            try {
                // 匿名登录
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                return true;
            }catch (AuthenticationException e) {
                // 捕获 AuthenticationException
                var reason = $"{e.Message} ({e.InnerException?.Message})";
                Debug.Log("登录失败:" + reason);
                return false;
            }catch (Exception e) {
                throw;
            }

        }

    }
}