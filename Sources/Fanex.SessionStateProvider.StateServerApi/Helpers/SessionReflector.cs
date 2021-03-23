namespace Fanex.SessionStateProvider.StateServerApi.Helpers
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Web;
    using System.Web.SessionState;
    using Fanex.SessionStateProvider.StateServerApi.Models;

    using Serializer = MessagePack.MessagePackSerializer.Typeless;

    public static class SessionReflector
    {
        private static readonly FieldInfo SessionStoreFieldInfo = typeof(SessionStateModule).GetField("_store", BindingFlags.NonPublic | BindingFlags.Instance);

        public static Dictionary<string, byte[]> GetSession(HttpContext context, string sessionId)
        {
            var sessionStateStore = ReflectSessionStateStore(context);

            if (sessionStateStore == null)
            {
                return default;
            }

            var session = sessionStateStore.GetItem(
                context,
                sessionId,
                out _,
                out _,
                out _,
                out _);

            return MapSession(session);
        }

        private static Dictionary<string, byte[]> MapSession(SessionStateStoreData session)
        {
            var result = new Dictionary<string, byte[]>();

            if (session != null)
            {
                foreach (string key in session.Items.Keys)
                {
                    result[key] = Serializer.Serialize(session.Items[key]);
                }
            }

            return result;
        }

        public static void Refresh(HttpContext context, string sessionId)
            => ReflectSessionStateStore(context)?.ResetItemTimeout(context, sessionId);

        public static void SetSession(HttpContext context, string sessionId, SessionCommit sessionCommit)
        {
            var sessionStateStore = ReflectSessionStateStore(context);

            if (sessionStateStore == null)
            {
                return;
            }

            var session = sessionStateStore.GetItemExclusive(
                context,
                sessionId,
                out bool locked,
                out _,
                out object lockId,
                out _);
            var newSession = session == null && !locked;

            session ??= sessionStateStore.CreateNewStoreData(context, (int)sessionCommit.SessionTimeout.TotalSeconds);

            UpdateSession(session, sessionCommit);
            sessionStateStore.SetAndReleaseItemExclusive(context, sessionId, session, lockId, newSession);
        }

        private static void UpdateSession(SessionStateStoreData session, SessionCommit sessionCommit)
        {
            if (sessionCommit.DeletedKeys != null && sessionCommit.DeletedKeys.Count > 0)
            {
                foreach (var deletedKey in sessionCommit.DeletedKeys)
                {
                    session.Items.Remove(deletedKey);
                }
            }

            if (sessionCommit.ModifiedItems != null && sessionCommit.ModifiedItems.Count > 0)
            {
                foreach (var item in sessionCommit.ModifiedItems)
                {
                    session.Items[item.Key] = Serializer.Deserialize(item.Value);
                }
            }

            session.Timeout = (int)sessionCommit.SessionTimeout.TotalMinutes;
        }

        private static SessionStateStoreProviderBase ReflectSessionStateStore(HttpContext context)
        {
            const string sessionModuleKey = "Session";

            if (context.ApplicationInstance.Modules[sessionModuleKey] is SessionStateModule sessionStateModule)
            {
                return SessionStoreFieldInfo.GetValue(sessionStateModule) as SessionStateStoreProviderBase;
            }

            return default;
        }
    }
}