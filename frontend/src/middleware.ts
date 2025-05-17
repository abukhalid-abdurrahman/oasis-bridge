import { NextRequest, NextResponse } from "next/server";

export function middleware(req: NextRequest) {
  const token = req.cookies.get("oasisToken")?.value;

  const { pathname, searchParams } = req.nextUrl;

  if (token && (searchParams.has("signin") || searchParams.has("signup"))) {
    const url = new URL("/", req.url);
    return NextResponse.redirect(url);
  }

  if (!token && pathname !== "/") {
    const url = new URL("/", req.url);
    url.searchParams.set("signin", "true");
    url.searchParams.set("callbackUrl", pathname);
    return NextResponse.redirect(url);
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/((?!_next|public|api|favicon.ico|.*\\..*).*)"],
};
