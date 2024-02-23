using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace WarGame_True.Infrastructure.Auth {
    /// <summary>
    /// ��װ unity - authentication ��
    /// </summary>
    public class WarAuthService {

        /// <summary>
        /// ����һ�� InitializationOptions ��
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
        /// ��ʼ��Unity Gaming Service
        /// </summary>
        /// <param name="options"></param>
        public async void InitAuthService(InitializationOptions options) {
            try {
                await UnityServices.InitializeAsync(options);

                if (!AuthenticationService.Instance.IsSignedIn) {
                    // δ��¼ ��������¼
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                Debug.Log("��ʼ��UGS��ɣ�");
            } catch (Exception e) {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// ������ҵ���Ϣ��Profile��
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public async Task SwitchProfileAndResignInAsync(string profile) {
            if (AuthenticationService.Instance.IsSignedIn) {
                // ����Ҫע���� �ſ��Ը���������Ϣ
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
                // ȷ���Ѿ���¼
                return true;
            }

            try {
                // ������¼
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                return true;
            }catch (AuthenticationException e) {
                // ���� AuthenticationException
                var reason = $"{e.Message} ({e.InnerException?.Message})";
                Debug.Log("��¼ʧ��:" + reason);
                return false;
            }catch (Exception e) {
                throw;
            }

        }

    }
}