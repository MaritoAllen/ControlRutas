// Decompiled with JetBrains decompiler
// Type: ControlRutas.Services.FirebaseService
// Assembly: ControlRutas, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F39A1E1-5A70-434C-A085-67C4347410AA
// Assembly location: C:\Users\Mario Braham\Downloads\ControlRutas.dll

using FirebaseAdmin;
using FirebaseAdmin.Auth;
using FirebaseAdmin.Messaging;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Google.Apis.Auth.OAuth2;
using System;
using System.Net;
using System.Net.Security;
using System.Threading.Tasks;
namespace ControlRutas.Services
{
    public class FirebaseService
    {
        private readonly IFirebaseConfig _config;
        private readonly IFirebaseClient _client;
        private readonly FirebaseApp _app;

        public FirebaseService()
        {
            ServicePointManager.ServerCertificateValidationCallback += (RemoteCertificateValidationCallback)((sender, certificate, chain, sslPolicyErrors) => true);
            this._config = (IFirebaseConfig)new FirebaseConfig()
            {
                AuthSecret = "AIzaSyBdk4qPoGh0BPh_k7VyNXTpHWEWtJb-PdY",
                BasePath = "https://bd-pruebas-rutas-default-rtdb.firebaseio.com/"
            };
            this._client = (IFirebaseClient)new FirebaseClient(this._config);
            if (FirebaseApp.DefaultInstance == null)
                this._app = FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(AppContext.BaseDirectory + "Services/bd-pruebas-rutas-firebase-adminsdk-pf71t-df941c8935.json")
                });
            else
                this._app = FirebaseApp.DefaultInstance;
            if (this._client == null)
                throw new Exception("No se pudo conectar a Firebase.");
        }

        public async Task<UserRecord> RegisterUserAsync(string email, string password, string guid)
        {
            UserRecordArgs userRecordArgs = new UserRecordArgs()
            {
                Email = email,
                Password = password,
                Uid = guid
            };
            UserRecord response = (UserRecord)null;
            try
            {
                response = await ((AbstractFirebaseAuth)FirebaseAuth.DefaultInstance).CreateUserAsync(userRecordArgs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            UserRecord userRecord = response;
            response = (UserRecord)null;
            return userRecord;
        }

        public async Task<SetResponse> NuevaRuta(string guid, string latitud, string longitud)
        {
            var data = new
            {
                Latitud = latitud,
                Longitud = longitud,
                Estado = "Activa"
            };
            return await this._client.SetAsync("Rutas/" + guid, data);
        }

        public async Task<FirebaseResponse> ActualizarRuta(string guid, string estado)
        {
            var data = new { Estado = estado };
            return await this._client.UpdateAsync("Rutas/" + guid, data);
        }

        public async Task<SetResponse> AgregarHijoRuta(
          string guidRuta,
          string guidHijo,
          string latitudInicio,
          string longitudInicio,
          string latitudFin,
          string longitudFin)
        {
            var data = new
            {
                LatitudInicio = latitudInicio,
                LongitudInicio = longitudInicio,
                LatitudFin = latitudFin,
                LongitudFin = longitudFin
            };
            return await this._client.SetAsync($"Rutas/{guidRuta}/Hijos/{guidHijo}", data);
        }

        public async Task<string> EnviarNotificacion(string mensaje, string token)
        {
            FirebaseMessaging.GetMessaging(this._app);
            return await FirebaseMessaging.DefaultInstance.SendAsync(new Message()
            {
                Token = token,
                Notification = new Notification()
                {
                    Title = "Control de Rutas",
                    Body = mensaje
                }
            });
        }
    }
}
