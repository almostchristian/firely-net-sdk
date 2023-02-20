﻿/*
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 *
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using Hl7.Fhir.Utility;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Hl7.Fhir.Rest
{
    internal class HttpClientRequester : IDisposable
    {
        public FhirClientSettings Settings { get; set; }
        public Uri BaseUrl { get; private set; }
        public HttpClient Client { get; private set; }
        private readonly bool _disposeHttpClient = true;

        public HttpClientRequester(Uri baseUrl, FhirClientSettings settings, HttpMessageHandler messageHandler, bool disposeHandler = true)
        {
            Settings = settings;
            BaseUrl = baseUrl;

            Client = new HttpClient(messageHandler, disposeHandler);
            Client.DefaultRequestHeaders.Add("User-Agent", $".NET FhirClient for FHIR");
            Client.Timeout = TimeSpan.FromMilliseconds(Settings.Timeout);
        }

        public HttpClientRequester(Uri baseUrl, FhirClientSettings settings, HttpClient client)
        {
            Settings = settings;
            BaseUrl = baseUrl;

            Client = client;
            _disposeHttpClient = false;
        }

        public async Task<EntryResponse> ExecuteAsync(EntryRequest interaction, CancellationToken ct)
        {
            if (interaction == null) throw Error.ArgumentNull(nameof(interaction));
          
            using var requestMessage = interaction.ToHttpRequestMessage(BaseUrl, Settings);
            if (Settings.PreferCompressedResponses)
            {
                requestMessage.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                requestMessage.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            }

            byte[] outgoingBody = null;
            if (requestMessage.Content is not null && (requestMessage.Method == HttpMethod.Post || requestMessage.Method == HttpMethod.Put))
            {
#if NET6_0_OR_GREATER
                outgoingBody = await requestMessage.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
#else
                outgoingBody = await requestMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
#endif
            }

#if NET6_0_OR_GREATER
            using var response = await Client.SendAsync(requestMessage,ct).ConfigureAwait(false);
#else
            using var response = await Client.SendAsync(requestMessage).ConfigureAwait(false);
#endif

            try
            {
#if NET6_0_OR_GREATER
                var body = await response.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
#else
                var body = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
#endif
                return response.ToEntryResponse(body);
            }
            catch (AggregateException ae)
            {
                throw ae.GetBaseException();
            }
        }

#region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing && _disposeHttpClient)
                {
                    // Only dispose the httpclient if was created here
                    this.Client.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
#endregion
    }

}