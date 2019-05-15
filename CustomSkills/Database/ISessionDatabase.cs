using System;

namespace CustomSkills.Database
{
    public interface ISessionDatabase
    {
        string ConnectionString { get; }

        Session Load(string userName);
        void Save(string userName, Session session);
    }
}