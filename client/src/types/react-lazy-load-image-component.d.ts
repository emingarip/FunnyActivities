declare module 'react-lazy-load-image-component' {
  import { ComponentType, ReactNode, CSSProperties, SyntheticEvent } from 'react';

  export interface LazyLoadImageProps extends React.ImgHTMLAttributes<HTMLImageElement> {
    effect?: string;
    placeholder?: ReactNode;
    placeholderSrc?: string;
    threshold?: number;
    delayMethod?: 'debounce' | 'throttle';
    delayTime?: number;
    useIntersectionObserver?: boolean;
    visibleByDefault?: boolean;
    wrapperClassName?: string;
    wrapperProps?: Record<string, any>;
    afterLoad?: () => void;
    beforeLoad?: () => void;
    scrollPosition?: { x: number; y: number };
  }

  export const LazyLoadImage: ComponentType<LazyLoadImageProps>;

  // Add other exports if needed
  export function trackWindowScroll<P extends object>(
    BaseComponent: ComponentType<P>
  ): ComponentType<P & { scrollPosition?: { x: number; y: number } }>;

  export const LazyLoadComponent: ComponentType<any>; // Placeholder, add proper types if used
}