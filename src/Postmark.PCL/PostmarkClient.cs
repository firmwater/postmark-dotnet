﻿using PostmarkDotNet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PostmarkDotNet
{
    /// <summary>
    /// The main entry point to Postmark.
    /// </summary>
    public class PostmarkClient : PostmarkClientBase
    {
        protected override string AuthHeaderName
        {
            get { return "X-Postmark-Server-Token"; }
        }

        /// <summary>
        /// Instantiate the client.
        /// </summary>
        /// <param name="serverToken"></param>
        public PostmarkClient(string serverToken)
        {
            _authToken = serverToken;
        }

        /// <summary>
        /// Sends a message through the Postmark API.
        /// All email addresses must be valid, and the sender must be
        /// a valid sender signature according to Postmark. To obtain a valid
        /// sender signature, log in to Postmark and navigate to:
        /// http://postmarkapp.com/signatures.
        /// </summary>
        /// <param name="message">A prepared message instance.</param>
        /// <returns></returns>
        public async Task<PostmarkResponse> SendMessageAsync(PostmarkMessage message)
        {
            return await ProcessRequestAsync<PostmarkMessage, PostmarkResponse>("/email", HttpMethod.Post, message);
        }

        /// <summary>
        /// Sends a batch of up to 500 messages through the Postmark API.
        /// All email addresses must be valid, and the sender must be
        /// a valid sender signature according to Postmark. To obtain a valid
        /// sender signature, log in to Postmark and navigate to:
        /// http://postmarkapp.com/signatures.
        /// </summary>
        /// <param name="messages">A prepared message batch.</param>
        /// <returns></returns>
        public async Task<IEnumerable<PostmarkResponse>> SendMessagesAsync(params PostmarkMessage[] messages)
        {
            return await ProcessRequestAsync<PostmarkMessage[], PostmarkResponse[]>("/email", HttpMethod.Post, messages);
        }

        /// <summary>
        /// Retrieves the bounce-related <see cref = "PostmarkDeliveryStats" /> results for the
        /// associated mail server.
        /// </summary>
        /// <returns></returns>
        public async Task<PostmarkDeliveryStats> GetDeliveryStatsAsync()
        {
            return await this.ProcessNoBodyRequestAsync<PostmarkDeliveryStats>("/deliverystats");
        }

        /// <summary>
        /// Retrieves a collection of <see cref = "PostmarkBounce" /> instances along
        /// with a sum total of bounces recorded by the server, based on filter parameters.
        /// </summary>
        /// <param name="type">The type of bounces to filter on</param>
        /// <param name="inactive">Whether to return only inactive or active bounces; use null to return all bounces</param>
        /// <param name="emailFilter">Filters based on whether the filter value is contained in the bounce source's email</param>
        /// <param name="tag">Filters on the bounce tag</param>
        /// <param name="messageID">Filters by messageID</param>
        /// <param name="offset">The page offset for the returned results; defaults to 0.</param>
        /// <param name="count">The number of results to return by the page offset; defaults to 100.</param>
        /// <returns></returns>
        /// <seealso href = "http://developer.postmarkapp.com/bounces" />
        public async Task<PostmarkBounces> GetBouncesAsync(int offset = 0, int count = 100, PostmarkBounceType? type = null,
            bool? inactive = null, string emailFilter = null, string tag = null, string messageID = null)
        {
            var parameters = new Dictionary<string, object>();
            parameters["type"] = type;
            parameters["inactive"] = type;
            parameters["emailFilter"] = emailFilter;
            parameters["tag"] = tag;
            parameters["messageID"] = messageID;
            parameters["offset"] = offset;
            parameters["count"] = count;

            return await ProcessNoBodyRequestAsync<PostmarkBounces>("/bounces", parameters);
        }

        /// <summary>
        /// Retrieves a single <see cref = "PostmarkBounce" /> based on a specified ID.
        /// </summary>
        /// <param name="bounceId">The bounce ID</param>
        /// <returns></returns>
        /// <seealso href = "http://developer.postmarkapp.com/bounces" />
        public async Task<PostmarkBounce> GetBounceAsync(string bounceId)
        {
            return await ProcessNoBodyRequestAsync<PostmarkBounce>("/bounce/" + bounceId);
        }

        /// <summary>
        /// Returns the raw source of the bounce we accepted. 
        /// If Postmark does not have a dump for that bounce, it will return an empty string.
        /// </summary>
        /// <param name="bounceId">The bounce ID</param>
        /// <returns></returns>
        /// <seealso href = "http://developer.postmarkapp.com/bounces" />
        public async Task<PostmarkBounceDump> GetBounceDumpAsync(string bounceId)
        {
            return await ProcessNoBodyRequestAsync<PostmarkBounceDump>("/bounces/" + bounceId + "/dump");
        }

        /// <summary>
        /// Activates a deactivated bounce.
        /// </summary>
        /// <param name="bounceId">The bounce ID</param>
        /// <returns></returns>
        /// <seealso href = "http://developer.postmarkapp.com/bounces" />
        public async Task<PostmarkBounceActivation> ActivateBounceAsync(string bounceId)
        {
            return await ProcessNoBodyRequestAsync<PostmarkBounceActivation>("/bounces/" + bounceId + "/activate");
        }

        /// <summary>
        /// Returns a list of tags used for the current server.
        /// </summary>
        /// <returns></returns>
        /// <seealso href = "http://developer.postmarkapp.com/bounces" />
        public async Task<IEnumerable<string>> GetBounceTagsAsync()
        {
            return await ProcessNoBodyRequestAsync<String[]>("/bounces/tags");
        }


        /// <summary>
        /// Return a listing of Outbound sent messages using the filters supported by the API.
        /// </summary>
        /// <param name="recipient">Filter by the recipient(s) of the message.</param>
        /// <param name="fromemail">Filter by the email address the message is sent from.</param>
        /// <param name="tag">Filter by a tag used for the message (messages sent directly through the API only)</param>
        /// <param name="subject">Filter by message subject.</param>
        /// <param name="count">Number of messages to return per call. (required)</param>
        /// <param name="offset">Number of messages to offset/page per call. (required)</param>
        /// <returns>PostmarkOutboundMessageList</returns>
        public async Task<PostmarkOutboundMessageList> GetOutboundMessagesAsync(int count = 100, int offset = 0,
            string recipient = null, string fromemail = null, string tag = null, string subject = null)
        {
            var parameters = new Dictionary<string, object>();
            parameters["count"] = count;
            parameters["offset"] = offset;
            parameters["recipient"] = recipient;
            parameters["fromemail"] = fromemail;
            parameters["tag"] = tag;
            parameters["subject"] = subject;

            return await ProcessNoBodyRequestAsync<PostmarkOutboundMessageList>("/messages/outbound");
        }

        /// <summary>
        /// Get the full details of a sent message including all fields, raw body, attachment names, etc
        /// </summary>
        /// <param name="messageID">The MessageID of a message which can be optained either from the initial API send call or a GetOutboundMessages call.</param>
        /// <returns>OutboundMessageDetail</returns>
        public async Task<OutboundMessageDetail> GetOutboundMessageDetailsAsync(string messageID)
        {
            return await ProcessNoBodyRequestAsync<OutboundMessageDetail>("/messages/outbound/" + messageID + "/details");
        }

        /// <summary>
        /// Get the original raw message dump of on outbound message including all SMTP headers and data.
        /// </summary>
        /// <param name="messageID">The MessageID of a message which can be optained either from the initial API send call or a GetOutboundMessages call.</param>
        /// <returns>MessageDump</returns>
        public async Task<MessageDump> GetOutboundMessageDumpAsync(string messageID)
        {
            return await ProcessNoBodyRequestAsync<MessageDump>("/messages/outbound/" + messageID + "/dump");
        }


        /// <summary>
        /// Return a listing of Inbound sent messages using the filters supported by the API.
        /// </summary>
        /// <param name="recipient">Filter by the recipient(s) of the message.</param>
        /// <param name="fromemail">Filter by the email address the message is sent from.</param>
        /// <param name="subject">Filter by message subject.</param>
        /// <param name="mailboxhash">Filter by mailbox hash that was parsed from the inbound message.</param>
        /// <param name="count">Number of messages to return per call. (required)</param>
        /// <param name="offset">Number of messages to offset/page per call. (required)</param>
        /// <returns>PostmarkInboundMessageList</returns>
        public async Task<PostmarkInboundMessageList> GetInboundMessagesAsync(int count = 100, int offset = 0,
            string recipient = null, string fromemail = null, string subject = null, string mailboxhash = null)
        {
            var parameters = new Dictionary<string, object>();
            parameters["count"] = count;
            parameters["offset"] = offset;
            parameters["recipient"] = recipient;
            parameters["fromemail"] = fromemail;
            parameters["subject"] = subject;
            parameters["mailboxhash"] = mailboxhash;

            return await ProcessNoBodyRequestAsync<PostmarkInboundMessageList>("/messages/inbound", parameters);
        }

        /// <summary>
        /// Get the full details of a processed inbound message including all fields, attachment names, etc.
        /// </summary>
        /// <param name="messageID">The MessageID of a message which can be optained either from the initial API send call or a GetInboundMessages call.</param>
        /// <returns>InboundMessageDetail</returns>
        public async Task<InboundMessageDetail> GetInboundMessageDetailsAsync(string messageID)
        {
            return await ProcessNoBodyRequestAsync<InboundMessageDetail>("/messages/inbound/" + messageID + "/details");
        }


        /// <summary>
        /// Gets the server associated with this client based on 
        /// the ServerToken supplied when the client was constructed.
        /// </summary>
        /// <returns></returns>
        public async Task<PostmarkServer> GetServer()
        {
            return await this.ProcessNoBodyRequestAsync<PostmarkServer>("/server");
        }

        /// <summary>
        /// Updates the server associated with this client. Only parameters that are passed into this method are modified. 
        /// Any parameters that are left null will use the current value for the server.
        /// </summary>
        /// <returns></returns>
        public async Task<PostmarkServer> EditServer(String name = null, ServerColors? color = null,
            bool? rawEmailEnabled = null, bool? smtpApiActivated = null, string inboundHookUrl = null,
            string bounceHookUrl = null, string openHookUrl = null, bool? postFirstOpenOnly = null,
            bool? trackOpens = null, string inboundDomain = null, int? inboundSpamThreshold = null)
        {
            var body = new Dictionary<string, object>();
            body["Name"] = name;
            body["Color"] = color;
            body["RawEmailEnabled"] = rawEmailEnabled;
            body["SmtpApiActivated"] = smtpApiActivated;
            body["InboundHookUrl"] = inboundHookUrl;
            body["BounceHookUrl"] = bounceHookUrl;
            body["OpenHookUrl"] = openHookUrl;
            body["PostFirstOpenOnly"] = postFirstOpenOnly;
            body["TrackOpens"] = trackOpens;
            body["InboundDomain"] = inboundDomain;
            body["InboundSpamThreshold"] = inboundSpamThreshold;

            body = body.Where(kv => kv.Value != null).ToDictionary(k => k.Key, v => v.Value);

            return await this.ProcessRequestAsync<Dictionary<string, object>, PostmarkServer>("/server", HttpMethod.Put, body);
        }

        /// <summary>
        /// Bypass rules for a blocked inbound message.
        /// </summary>
        /// <param name="messageid"></param>
        /// <returns></returns>
        public async Task<PostmarkResponse> BypassBlockedInboundMessage(string messageid)
        {
            return await this.ProcessNoBodyRequestAsync<PostmarkResponse>(String.Format("/messages/inbound/{0}/bypass", messageid), verb: HttpMethod.Put);
        }

        /// <summary>
        /// Get the Open Events for messages, optionally filtering by various
        /// attributes of the Open Events and Messages.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="recipient"></param>
        /// <param name="tag"></param>
        /// <param name="clientName"></param>
        /// <param name="clientCompany"></param>
        /// <param name="clientFamily"></param>
        /// <param name="operatingSystemName"></param>
        /// <param name="operatingSystemFamily"></param>
        /// <param name="operatingSystemCompany"></param>
        /// <param name="platform"></param>
        /// <param name="country"></param>
        /// <param name="region"></param>
        /// <param name="city"></param>
        /// <returns></returns>
        public async Task<PostmarkOpensList> GetOpenEventsForMessagesAsync(
            int offset = 0, int count = 100, string recipient = null, string tag = null,
            string clientName = null, string clientCompany = null, string clientFamily = null,
            string operatingSystemName = null, string operatingSystemFamily = null, string operatingSystemCompany = null,
            string platform = null, string country = null, string region = null, string city = null)
        {
            var parameters = new Dictionary<string, object>();
            parameters["offset"] = offset;
            parameters["count"] = count;
            parameters["recipient"] = recipient;
            parameters["tag"] = tag;
            parameters["client_name"] = clientName;
            parameters["client_company"] = clientCompany;
            parameters["client_family"] = clientFamily;
            parameters["os_name"] = operatingSystemName;
            parameters["os_family"] = operatingSystemFamily;
            parameters["os_company"] = operatingSystemCompany;
            parameters["platform"] = platform;
            parameters["country"] = country;
            parameters["region"] = region;
            parameters["city"] = city;

            return await this
                .ProcessNoBodyRequestAsync<PostmarkOpensList>("/messages/outbound/opens", parameters);
        }

        /// <summary>
        /// Get the Open events for a specific message.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<PostmarkOpensList> GetOpenEventsForMessageAsync(
            string messageId, int offset = 0, int count = 100)
        {

            var parameters = new Dictionary<string, object>();
            parameters["offset"] = offset;
            parameters["count"] = count;

            return await this.ProcessNoBodyRequestAsync<PostmarkOpensList>
                (String.Format("/messages/outbound/opens/{0}", messageId), parameters);
        }

        private IDictionary<string, object>
           ConstructSentStatsFilter(string tag, DateTime? fromDate, DateTime? toDate)
        {
            var parameters = new Dictionary<string, object>();
            parameters["tag"] = tag;
            if (fromDate.HasValue)
            {
                parameters["fromdate"] = fromDate.Value.ToString(DATE_FORMAT);
            }
            if (toDate.HasValue)
            {
                parameters["todate"] = toDate.Value.ToString(DATE_FORMAT);
            }
            return parameters;
        }


        /// <summary>
        /// Get an overview of outbound statistics, optionally limiting by tag or time window.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public async Task<PostmarkOutboundOverviewStats> GetOutboundOverviewStatsAsync(
            string tag = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var parameters = ConstructSentStatsFilter(tag, fromDate, toDate);
            return await this.ProcessNoBodyRequestAsync<PostmarkOutboundOverviewStats>
                ("/stats/outbound", parameters);
        }

        public async Task<PostmarkOutboundSentStats>
            GetOutboundSentCountsAsync(string tag = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var parameters = ConstructSentStatsFilter(tag, fromDate, toDate);
            return await this.ProcessNoBodyRequestAsync<PostmarkOutboundSentStats>
                ("/stats/outbound/sends", parameters);
        }

        public async Task<PostmarkOutboundBounceStats>
            GetOutboundSentCountsAsync(string tag = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var parameters = ConstructSentStatsFilter(tag, fromDate, toDate);
            return await this.ProcessNoBodyRequestAsync<PostmarkOutboundBounceStats>
                ("/stats/outbound/bounces", parameters);
        }

        public async Task<PostmarkOutboundSpamComplaintStats> GetOutboundSpamComplaintCountsAsync(string tag = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var parameters = ConstructSentStatsFilter(tag, fromDate, toDate);
            return await this.ProcessNoBodyRequestAsync<PostmarkOutboundSpamComplaintStats>
                ("/stats/outbound/spam", parameters);
        }

        public async Task<PostmarkOutboundTrackedStats> GetOutboundTrackingCountsAsync(string tag = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var parameters = ConstructSentStatsFilter(tag, fromDate, toDate);
            return await this.ProcessNoBodyRequestAsync<PostmarkOutboundTrackedStats>
                ("/stats/outbound/tracked", parameters);
        }

        public async Task<PostmarkOutboundOpenStats> GetOutboundOpenCountsAsync(string tag = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var parameters = ConstructSentStatsFilter(tag, fromDate, toDate);
            return await this.ProcessNoBodyRequestAsync<PostmarkOutboundOpenStats>
                ("/stats/outbound/opens", parameters);
        }

        public async Task<PostmarkOutboundPlatformStats> GetOutboundPlatformCountsAsync(string tag = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var parameters = ConstructSentStatsFilter(tag, fromDate, toDate);
            return await this.ProcessNoBodyRequestAsync<PostmarkOutboundPlatformStats>
                ("/stats/outbound/platforms", parameters);
        }

        public async Task<PostmarkTaggedTriggerInfo> CreateTagTriggerAsync(string matchName, bool trackOpens = true)
        {
            var parameters = new Dictionary<string, object>();
            parameters["MatchName"] = matchName;
            parameters["TrackOpens"] = trackOpens;

            return await this.ProcessRequestAsync<Dictionary<string, object>, PostmarkTaggedTriggerInfo>
                ("/triggers/tags", HttpMethod.Post, parameters);
        }

        public async Task<PostmarkTaggedTriggerInfo> GetTagTriggerAsync(int id)
        {
            var result = await this.ProcessNoBodyRequestAsync<PostmarkTaggedTriggerInfo>(String.Format("/triggers/tags/{0}", id));
            result.ID = id;
            return result;
        }

        public async Task<PostmarkTaggedTriggerInfo> UpdateTagTriggerAsync(int id, string matchName = null, bool? trackOpens = null)
        {
            var parameters = new Dictionary<string, object>();
            parameters["MatchName"] = matchName;
            parameters["TrackOpens"] = trackOpens;

            var result = await this
                .ProcessNoBodyRequestAsync<PostmarkTaggedTriggerInfo>
                ("/triggers/tags/" + id, parameters, HttpMethod.Put);

            result.ID = id;
            return result;
        }

        public async Task<PostmarkResponse> DeleteTagTrigger(int id)
        {
            return await this
                .ProcessNoBodyRequestAsync<PostmarkResponse>("/triggers/tags/" + id,
                verb: HttpMethod.Delete);
        }

        public async Task<PostmarkTaggedTriggerList> SearchTaggedTriggers(int offset = 0, int count = 100, string matchName = null)
        {
            var parameters = new Dictionary<string, object>();
            parameters["offset"] = offset;
            parameters["count"] = count;
            parameters["match_name"] = matchName;

            return await this.ProcessNoBodyRequestAsync<PostmarkTaggedTriggerList>("/triggers/tags/", parameters);
        }
    }
}