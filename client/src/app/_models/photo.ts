export interface Photo {
    id: number;
    url: string;
    isMain: boolean;
    publicId?: any;
    isApproved: boolean;
    username?: string;
    userId?: string,
    marked?: boolean;
}