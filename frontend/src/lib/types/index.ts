export type searchParams = {
  signin?: string;
  signup?: string;
};

export interface User {
  token: string;
  expiresAt: string;
  startTime: string;
  Id: string;
  UserName: string;
  Email: string;
}