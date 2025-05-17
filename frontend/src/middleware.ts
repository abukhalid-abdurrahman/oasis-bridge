import { NextRequest, NextResponse } from "next/server";

export function middleware(req: NextRequest) {
  const token = req.cookies.get("oasisToken")?.value;
  const { pathname, search } = req.nextUrl;

  const isAuthPage = pathname === "/signin" || pathname === "/signup";

  if (token && isAuthPage) {
    return NextResponse.redirect(new URL("/", req.url));
  }

  if (!token && pathname !== "/") {
    const url = new URL("/", req.url);
    url.searchParams.set("signin", "true");
    url.searchParams.set("callbackUrl", pathname + search);
    return NextResponse.redirect(url);
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico|.*\\..*|api).*)"],
};
