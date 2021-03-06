using System.Collections.Generic;
using System.Threading;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.Jwt;

namespace TodoListService
{
  // This class is necessary because the OAuthBearer Middleware does not leverage
  // the OpenID Connect metadata endpoint exposed by the STS by default.

  public class OpenIdConnectSecurityTokenProvider : IIssuerSecurityKeyProvider
  {
    public ConfigurationManager<OpenIdConnectConfiguration> ConfigManager;
    private string _issuer;
    //private IEnumerable<SecurityToken> _tokens;
    private IEnumerable<SecurityKey> _keys;
    private readonly string _metadataEndpoint;

    private readonly ReaderWriterLockSlim _synclock = new ReaderWriterLockSlim();

    public OpenIdConnectSecurityTokenProvider(string metadataEndpoint)
    {
      _metadataEndpoint = metadataEndpoint;
      ConfigManager = new ConfigurationManager<OpenIdConnectConfiguration>(metadataEndpoint);

      RetrieveMetadata();
    }

    /// <summary>
    /// Gets the issuer the credentials are for.
    /// </summary>
    /// <value>
    /// The issuer the credentials are for.
    /// </value>
    public string Issuer
    {
      get
      {
        RetrieveMetadata();
        _synclock.EnterReadLock();
        try
        {
          return _issuer;
        }
        finally
        {
          _synclock.ExitReadLock();
        }
      }
    }

    public IEnumerable<SecurityKey> SecurityKeys
    {
      get
      {
        RetrieveMetadata();
        _synclock.EnterReadLock();
        try
        {
          return _keys;
        }
        finally
        {
          _synclock.ExitReadLock();
        }
      }
    }

    ///// <summary>
    ///// Gets all known security tokens.
    ///// </summary>
    ///// <value>
    ///// All known security tokens.
    ///// </value>
    //public IEnumerable<SecurityToken> SecurityTokens
    //    {
    //        get
    //        {
    //            RetrieveMetadata();
    //            _synclock.EnterReadLock();
    //            try
    //            {
    //                return _tokens;
    //            }
    //            finally
    //            {
    //                _synclock.ExitReadLock();
    //            }
    //        }
    //    }

    private void RetrieveMetadata()
    {
      _synclock.EnterWriteLock();
      try
      {
        OpenIdConnectConfiguration config = ConfigManager.GetConfigurationAsync().Result;
        _issuer = config.Issuer;
        _keys = config.SigningKeys;
      }
      finally
      {
        _synclock.ExitWriteLock();
      }
    }
  }
}
