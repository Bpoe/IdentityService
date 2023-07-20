// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.

// Adapted from https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/keyvault/Microsoft.Azure.KeyVault/src/Customized/Authentication/HttpBearerChallenge.cs

namespace Identity;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;

/// <summary>
/// Handles http bearer challenge operations
/// </summary>
public sealed class HttpBearerChallenge
{
    private const string Authorization = "authorization";
    private const string AuthorizationUri = "authorization_uri";
    private const string Bearer = "Bearer";

    private readonly IDictionary<string, string> parameters = new Dictionary<string, string>();

    /// <summary>
    /// Parses an HTTP WWW-Authentication Bearer challenge from a server.
    /// </summary>
    /// <param name="challenge">The AuthenticationHeaderValue to parse</param>
    public HttpBearerChallenge(Uri requestUri, string challenge)
    {
        string authority = ValidateRequestURI(requestUri);
        string trimmedChallenge = ValidateChallenge(challenge);

        SourceAuthority = authority;
        SourceUri = requestUri;

        // Split the trimmed challenge into a set of name=value strings that
        // are comma separated. The value fields are expected to be within
        // quotation characters that are stripped here.
        var pairs = trimmedChallenge.Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries);

        if (pairs != null && pairs.Length > 0)
        {
            // Process the name=value strings
            for (int i = 0; i < pairs.Length; i++)
            {
                var pair = pairs[i].Split('=');

                if (pair.Length == 2)
                {
                    // We have a key and a value, now need to trim and decode
                    var key = pair[0].Trim().Trim(new char[] { '\"' });
                    var value = pair[1].Trim().Trim(new char[] { '\"' });

                    if (!string.IsNullOrEmpty(key))
                    {
                        parameters[key] = value;
                    }
                }
            }
        }

        // Minimum set of parameters
        if (parameters.Count < 1)
            throw new ArgumentException("Invalid challenge parameters", "challenge");

        // Must specify authorization or authorization_uri
        if (!parameters.ContainsKey(Authorization) && !parameters.ContainsKey(AuthorizationUri))
            throw new ArgumentException("Invalid challenge parameters", "challenge");
    }

    /// <summary>
    /// Returns the value stored at the specified key.
    /// </summary>
    /// <remarks>
    /// If the key does not exist, will return false and the
    /// content of value will not be changed
    /// </remarks>
    /// <param name="key">The key to be retrieved</param>
    /// <param name="value">The value for the specified key</param>
    /// <returns>True when the key is found, false when it is not</returns>
    public bool TryGetValue(string key, out string value)
    {
        return parameters.TryGetValue(key, out value);
    }

    /// <summary>
    /// Returns the URI for the Authorization server if present,
    /// otherwise string.Empty
    /// </summary>
    public string AuthorizationServer
    {
        get
        {
            if (parameters.TryGetValue("authorization_uri", out string? value))
                return value ?? string.Empty;

            return parameters.TryGetValue("authorization", out value)
                ? value ?? string.Empty
                : string.Empty;
        }
    }

    /// <summary>
    /// Returns the Realm value if present, otherwise the Authority
    /// of the request URI given in the ctor
    /// </summary>
    public string? Resource
        => parameters.TryGetValue("resource", out string? value) && value is not null
                ? value
                : SourceAuthority;

    /// <summary>
    /// Returns the Scope value if present, otherwise string.Empty
    /// </summary>
    public string Scope
        => parameters.TryGetValue("scope", out string? value) ? value : string.Empty;

    /// <summary>
    /// The Authority of the request URI
    /// </summary>
    public string? SourceAuthority { get; } = null;

    /// <summary>
    /// The source URI
    /// </summary>
    public Uri? SourceUri { get; } = null;

    /// <summary>
    /// Tests whether an authentication header is a Bearer challenge
    /// </summary>
    /// <remarks>
    /// This method is forgiving: if the parameter is null, or the scheme
    /// in the header is missing, then it will simply return false.
    /// </remarks>
    /// <param name="challenge">The AuthenticationHeaderValue to test</param>
    /// <returns>True if the header is a Bearer challenge</returns>
    public static bool IsBearerChallenge(string challenge)
        => !string.IsNullOrEmpty(challenge) && challenge.Trim().StartsWith(Bearer + " ");

    public static HttpBearerChallenge? GetBearerChallengeFromResponse(HttpResponseMessage response)
    {
        if (response is null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        var challenge = response?.Headers.WwwAuthenticate.FirstOrDefault()?.ToString();

        return !string.IsNullOrEmpty(challenge) && IsBearerChallenge(challenge) && response?.RequestMessage?.RequestUri is not null
            ? new HttpBearerChallenge(response.RequestMessage.RequestUri, challenge)
            : null;
    }

    private static string ValidateChallenge(string challenge)
    {
        if (string.IsNullOrEmpty(challenge))
            throw new ArgumentNullException("challenge");

        string trimmedChallenge = challenge.Trim();

        if (!trimmedChallenge.StartsWith(Bearer + " "))
            throw new ArgumentException("Challenge is not Bearer", "challenge");

        return trimmedChallenge.Substring(Bearer.Length + 1);
    }

    private static string ValidateRequestURI(Uri requestUri)
    {
        if (null == requestUri)
            throw new ArgumentNullException("requestUri");

        if (!requestUri.IsAbsoluteUri)
            throw new ArgumentException("The requestUri must be an absolute URI", "requestUri");

        if (!requestUri.Scheme.Equals("http", StringComparison.CurrentCultureIgnoreCase) && !requestUri.Scheme.Equals("https", StringComparison.CurrentCultureIgnoreCase))
            throw new ArgumentException("The requestUri must be HTTP or HTTPS", "requestUri");

        return requestUri.FullAuthority();
    }
}

public static class UriExtensions
{
    /// <summary>
    /// Returns an authority string for URI that is guaranteed to contain
    /// a port number.
    /// </summary>
    /// <param name="uri">The Uri from which to compute the authority</param>
    /// <returns>The complete authority for the Uri</returns>
    public static string FullAuthority(this Uri uri)
    {
        string authority = uri.Authority;

        if (!authority.Contains(':') && uri.Port > 0)
        {
            // Append port for complete authority
            authority = string.Format("{0}:{1}", uri.Authority, uri.Port.ToString());
        }

        return authority;
    }
}