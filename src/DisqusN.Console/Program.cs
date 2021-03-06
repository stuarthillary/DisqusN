﻿using System;
using System.Configuration;
using System.Security.AccessControl;
using DisqusN;
using DisqusN.Resources.Forums;
using static LanguageExt.Prelude;

namespace DisqusN.Console
{
    public class Program
    {
        static void Main(string[] args)
        {
            DisqusClientConfigurationSection section = (DisqusClientConfigurationSection)ConfigurationManager.GetSection("AvailableDisqusClients");

            string clientName = string.Empty;
            string accessToken = string.Empty;
            string publicKey = string.Empty;
            string secretKey = string.Empty;

            foreach (var client in section.DisqusClients)
            {
                var disqusClientElement = (DisqusClientElement)client;
                clientName = disqusClientElement.Name;
                accessToken = disqusClientElement.AccessToken;
                publicKey = disqusClientElement.PublicKey;
                secretKey = disqusClientElement.PrivateKey;
            }

            var ownerAccessToken = new ApplicationOwnerAccessToken(accessToken);
            var apiPublicKey = new DisqusApiPublicKey(publicKey);

            var disqusClient = new DisqusClient(apiPublicKey, ownerAccessToken) { ApplyStrictResponseParsing = true };

            var parameters = new ForumListThreadsParameters("disqus")
            {
                Limit = 10,
                Order = ItemOrdering.Descending,
                Related = new []{ ForumRelated.Author, ForumRelated.Forum },
                Since = new DateTime(2013,1,1,0,0,0)
            };

            var query = parameters.GetResourceUriQuery();

            var forumListThreadsRequest = ForumRequests.ListThreads(parameters);

            var forumThreads = disqusClient.Request(forumListThreadsRequest);

            forumThreads.Match(Right: tds =>
                tds.Response.Match(
                    rhs =>
                        rhs.Response.ForEach(
                            t => System.Console.WriteLine("Id: {0}, Posts: {1}, Title: {2}, Author: {3}", t.CreatedAt, t.Posts, t.Link, t.Author.Match(rhs1=>rhs1.Name, lhs1=> lhs1.ToString()))),
                    lhs => System.Console.WriteLine(lhs.Message)),
                Left: er => { });

            

            var param1 = new ForumDetailsParameters("disqus")
            {
                Attach = new [] { ForumAttach.ForumIntegration }
            };
            var forumDetailsRequest = ForumRequests.Details(param1);
            var forumDetails = disqusClient.Request(forumDetailsRequest);

            System.Console.ReadKey();
        }
    }
}
