export class User {
  public organization: string = "";
  public email: string = "";
  public name: string = "";
  public profilePicture: string = "";
  public applications: string = "";
  public groups: string = "";
}

export interface SelectOption {
  id: string;
  name: string;
}
