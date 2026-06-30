namespace KrossSounds.Middleware;

public class SecurityHeadersMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var config = context.RequestServices.GetRequiredService<IConfiguration>();

        var headers = context.Response.Headers;
        // ── Supprimer les headers qui révèlent la stack ─────────────────────── 
        headers.Remove("Server"); 
        headers.Remove("X-Powered-By"); 
        headers.Remove("X-AspNet-Version"); 
        headers.Remove("X-AspNetMvc-Version");
        // ── HSTS (redondant si UseHsts() est actif, mais utile via reverse proxy) // maxAge=31536000 (1 an), includeSubDomains, preload 
        headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
        // ── Empêche l'intégration dans un iframe (clickjacking) 
        if (config.AddXFrameOptionsDeny())
        {
            headers["X-Frame-Options"] = "DENY";
        }
        // ── Empêche le MIME sniffing (exécution de fichiers mal typés) 
        headers["X-Content-Type-Options"] = "nosniff";
        // ── Contrôle les informations transmises dans le Referer 
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        // ── Désactive les API navigateur non utilisées 
        headers["Permissions-Policy"] = "geolocation=(), camera=(), microphone=(), payment=(), usb=(), magnetometer=(), gyroscope=(), accelerometer=()";
        // ── Cross-Origin Opener Policy : isole le contexte de navigation 
        headers["Cross-Origin-Opener-Policy"] = "same-origin";
        // ── Cross-Origin Embedder Policy (activer si toutes les ressources sont same-origin) // 
        headers["Cross-Origin-Embedder-Policy"] = "unsafe-none";
        // ── Cross-Origin Resource Policy 
        headers["Cross-Origin-Resource-Policy"] = "cross-origin";
        // ── X-XSS-Protection : intentionnellement désactivé // La valeur "1; mode=block" peut introduire des vulnérabilités XSS dans // certains navigateurs. La valeur "0" désactive le filtre XSS du navigateur // (obsolète dans Chrome/Firefox) — CSP doit être la seule protection XSS. 
        headers["X-XSS-Protection"] = "0";
        // ── Content Security Policy (CSP) ───────────────────────────────────── 
        // Adapté à Razor Pages / MVC avec Bootstrap CDN et scripts inline signés. 
        // Ajuster 'nonce' ou⚠️ 'script-src' selon votre cas réel. 
        headers["Content-Security-Policy"] = "default-src 'self'; " + 
                                             "script-src 'self' 'unsafe-inline'; " +
                                             "style-src 'self' 'unsafe-inline'; " + 
                                             "img-src 'self' data: blob:; " + 
                                             "font-src 'self'; " + 
                                             "connect-src 'self'; " + 
                                             "media-src 'self'; " + 
                                             "object-src 'none'; " + 
                                             "frame-src https://www.youtube.com/ https://youtube-nocookie.com;" + 
                                             "frame-ancestors 'self'; " + 
                                             "form-action 'self'; " + 
                                             "base-uri 'self'; " + 
                                             (config.UseHsts() ? "upgrade-insecure-requests;" : "");
        await next(context);
    }
}