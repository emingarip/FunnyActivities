export {};

declare global {
  interface GoogleCredentialResponse {
    credential?: string;
    select_by?: string;
  }

  interface GoogleIdConfiguration {
    client_id: string;
    callback: (response: GoogleCredentialResponse) => void | Promise<void>;
    ux_mode?: 'popup' | 'redirect';
  }

  interface GoogleButtonConfiguration {
    theme?: 'outline' | 'filled_blue' | 'filled_black';
    size?: 'large' | 'medium' | 'small';
    text?: 'signin_with' | 'signup_with' | 'continue_with' | 'signin';
    shape?: 'rectangular' | 'pill' | 'circle' | 'square';
    width?: string | number;
    logo_alignment?: 'left' | 'center';
  }

  interface Window {
    google?: {
      accounts: {
        id: {
          initialize: (config: GoogleIdConfiguration) => void;
          renderButton: (parent: HTMLElement, options: GoogleButtonConfiguration) => void;
        };
      };
    };
  }
}
