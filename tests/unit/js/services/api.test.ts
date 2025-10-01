import axios from 'axios';
import { materialsAPI, authAPI, userAPI, adminAPI, auditAPI } from '../api';

// Mock axios
jest.mock('axios');
const mockedAxios = axios as jest.Mocked<typeof axios>;

// Mock localStorage
const localStorageMock = {
  getItem: jest.fn(),
  setItem: jest.fn(),
  removeItem: jest.fn(),
  clear: jest.fn(),
};
global.localStorage = localStorageMock as any;

// Mock window.location
delete (global as any).window.location;
global.window.location = { href: 'http://localhost:3000' } as any;

describe('API Service', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    localStorageMock.getItem.mockClear();
    localStorageMock.setItem.mockClear();
    localStorageMock.removeItem.mockClear();
  });

  describe('materialsAPI', () => {
    const mockMaterialId = 'test-material-id';
    const mockPhotoId = 'test-photo-id';

    describe('uploadMaterialPhotos', () => {
      it('uploads photos successfully', async () => {
        const mockFiles = [
          new File(['test1'], 'test1.jpg', { type: 'image/jpeg' }),
          new File(['test2'], 'test2.jpg', { type: 'image/jpeg' }),
        ];

        const mockResponse = {
          data: {
            success: true,
            data: { photoUrls: ['url1.jpg', 'url2.jpg'] },
          },
        };

        mockedAxios.post.mockResolvedValue(mockResponse);

        const result = await materialsAPI.uploadMaterialPhotos(mockMaterialId, mockFiles);

        expect(mockedAxios.post).toHaveBeenCalledWith(
          `/materials/${mockMaterialId}/upload-photos`,
          expect.any(FormData),
          {
            headers: {
              'Content-Type': 'multipart/form-data',
            },
          }
        );

        expect(result).toEqual(mockResponse);
      });

      it('handles upload errors', async () => {
        const mockFiles = [new File(['test'], 'test.jpg', { type: 'image/jpeg' })];
        const mockError = new Error('Upload failed');

        mockedAxios.post.mockRejectedValue(mockError);

        await expect(materialsAPI.uploadMaterialPhotos(mockMaterialId, mockFiles))
          .rejects.toThrow('Upload failed');
      });

      it('creates FormData with correct files', async () => {
        const mockFiles = [
          new File(['test1'], 'test1.jpg', { type: 'image/jpeg' }),
          new File(['test2'], 'test2.jpg', { type: 'image/png' }),
        ];

        const mockResponse = {
          data: { success: true, data: { photoUrls: ['url1.jpg', 'url2.jpg'] } },
        };

        mockedAxios.post.mockResolvedValue(mockResponse);

        await materialsAPI.uploadMaterialPhotos(mockMaterialId, mockFiles);

        const formDataCall = mockedAxios.post.mock.calls[0][1] as FormData;
        expect(formDataCall).toBeInstanceOf(FormData);
        // Note: FormData contents can't be easily inspected in Jest
      });

      it('handles empty file array', async () => {
        const mockResponse = {
          data: { success: true, data: { photoUrls: [] } },
        };

        mockedAxios.post.mockResolvedValue(mockResponse);

        const result = await materialsAPI.uploadMaterialPhotos(mockMaterialId, []);

        expect(mockedAxios.post).toHaveBeenCalled();
        expect(result).toEqual(mockResponse);
      });
    });

    describe('getMaterial', () => {
      it('fetches material successfully', async () => {
        const mockResponse = {
          data: {
            success: true,
            data: {
              id: mockMaterialId,
              name: 'Test Material',
              photos: [
                { id: 'photo1', url: 'url1.jpg', filename: 'photo1.jpg' },
                { id: 'photo2', url: 'url2.jpg', filename: 'photo2.jpg' },
              ],
            },
          },
        };

        mockedAxios.get.mockResolvedValue(mockResponse);

        const result = await materialsAPI.getMaterial(mockMaterialId);

        expect(mockedAxios.get).toHaveBeenCalledWith(`/materials/${mockMaterialId}`);
        expect(result).toEqual(mockResponse);
      });

      it('handles fetch errors', async () => {
        const mockError = new Error('Material not found');
        mockedAxios.get.mockRejectedValue(mockError);

        await expect(materialsAPI.getMaterial(mockMaterialId))
          .rejects.toThrow('Material not found');
      });

      it('handles network errors', async () => {
        const networkError = {
          response: undefined,
          message: 'Network Error',
        };
        mockedAxios.get.mockRejectedValue(networkError);

        await expect(materialsAPI.getMaterial(mockMaterialId))
          .rejects.toThrow();
      });
    });

    describe('deleteMaterialPhoto', () => {
      it('deletes photo successfully', async () => {
        const mockResponse = {
          data: { success: true, message: 'Photo deleted successfully' },
        };

        mockedAxios.delete.mockResolvedValue(mockResponse);

        const result = await materialsAPI.deleteMaterialPhoto(mockMaterialId, mockPhotoId);

        expect(mockedAxios.delete).toHaveBeenCalledWith(`/materials/${mockMaterialId}/photos/${mockPhotoId}`);
        expect(result).toEqual(mockResponse);
      });

      it('handles delete errors', async () => {
        const mockError = new Error('Delete failed');
        mockedAxios.delete.mockRejectedValue(mockError);

        await expect(materialsAPI.deleteMaterialPhoto(mockMaterialId, mockPhotoId))
          .rejects.toThrow('Delete failed');
      });

      it('handles unauthorized delete attempts', async () => {
        const unauthorizedError = {
          response: { status: 403 },
          message: 'Forbidden',
        };
        mockedAxios.delete.mockRejectedValue(unauthorizedError);

        await expect(materialsAPI.deleteMaterialPhoto(mockMaterialId, mockPhotoId))
          .rejects.toThrow();
      });
    });

    describe('getMaterials', () => {
      it('fetches materials with default params', async () => {
        const mockResponse = {
          data: {
            success: true,
            data: {
              items: [{ id: '1', name: 'Material 1' }],
              page: 1,
              pageSize: 10,
              totalCount: 1,
              totalPages: 1,
            },
          },
        };

        mockedAxios.get.mockResolvedValue(mockResponse);

        const result = await materialsAPI.getMaterials();

        expect(mockedAxios.get).toHaveBeenCalledWith('/materials', { params: undefined });
        expect(result).toEqual(mockResponse);
      });

      it('fetches materials with custom params', async () => {
        const params = {
          page: 2,
          pageSize: 20,
          name: 'Test Material',
          sortBy: 'name' as const,
          sortOrder: 'desc' as const,
        };

        const mockResponse = {
          data: { success: true, data: { items: [], page: 2, pageSize: 20, totalCount: 0, totalPages: 0 } },
        };

        mockedAxios.get.mockResolvedValue(mockResponse);

        const result = await materialsAPI.getMaterials(params);

        expect(mockedAxios.get).toHaveBeenCalledWith('/materials', { params });
        expect(result).toEqual(mockResponse);
      });
    });

    describe('createMaterial', () => {
      it('creates material successfully', async () => {
        const materialData = {
          name: 'New Material',
          description: 'Test description',
          stockQuantity: 100,
          category: 'Test Category',
          unit: 'pieces',
        };

        const mockResponse = {
          data: {
            success: true,
            data: { id: 'new-id', ...materialData },
          },
        };

        mockedAxios.post.mockResolvedValue(mockResponse);

        const result = await materialsAPI.createMaterial(materialData);

        expect(mockedAxios.post).toHaveBeenCalledWith('/materials', materialData);
        expect(result).toEqual(mockResponse);
      });
    });

    describe('updateMaterial', () => {
      it('updates material successfully', async () => {
        const updateData = {
          name: 'Updated Material',
          stockQuantity: 150,
        };

        const mockResponse = {
          data: { success: true, message: 'Material updated successfully' },
        };

        mockedAxios.put.mockResolvedValue(mockResponse);

        const result = await materialsAPI.updateMaterial(mockMaterialId, updateData);

        expect(mockedAxios.put).toHaveBeenCalledWith(`/materials/${mockMaterialId}`, updateData);
        expect(result).toEqual(mockResponse);
      });
    });

    describe('deleteMaterial', () => {
      it('deletes material successfully', async () => {
        const mockResponse = {
          data: { success: true, message: 'Material deleted successfully' },
        };

        mockedAxios.delete.mockResolvedValue(mockResponse);

        const result = await materialsAPI.deleteMaterial(mockMaterialId);

        expect(mockedAxios.delete).toHaveBeenCalledWith(`/materials/${mockMaterialId}`);
        expect(result).toEqual(mockResponse);
      });
    });
  });

  describe('Authentication and Authorization', () => {
    describe('Request Interceptor', () => {
      it('adds authorization header for authenticated requests', async () => {
        localStorageMock.getItem.mockReturnValue('test-token');

        const mockResponse = { data: { success: true } };
        mockedAxios.get.mockResolvedValue(mockResponse);

        await materialsAPI.getMaterial('test-id');

        const callConfig = mockedAxios.get.mock.calls[0][1];
        expect(callConfig?.headers?.Authorization).toBe('Bearer test-token');
      });

      it('skips authorization header for anonymous endpoints', async () => {
        localStorageMock.getItem.mockReturnValue('test-token');

        const mockResponse = { data: { success: true } };
        mockedAxios.post.mockResolvedValue(mockResponse);

        await authAPI.login({ email: 'test@test.com', password: 'password' });

        const callConfig = mockedAxios.post.mock.calls[0][2];
        expect(callConfig?.headers?.Authorization).toBeUndefined();
      });

      it('handles missing auth token', async () => {
        localStorageMock.getItem.mockReturnValue(null);

        const mockResponse = { data: { success: true } };
        mockedAxios.get.mockResolvedValue(mockResponse);

        await materialsAPI.getMaterial('test-id');

        const callConfig = mockedAxios.get.mock.calls[0][1];
        expect(callConfig?.headers?.Authorization).toBeUndefined();
      });
    });

    describe('Response Interceptor', () => {
      it('handles successful responses', async () => {
        const mockResponse = {
          data: { success: true, data: { test: 'data' } },
          status: 200,
          config: { url: '/test' },
        };

        mockedAxios.get.mockResolvedValue(mockResponse);

        const result = await materialsAPI.getMaterial('test-id');

        expect(result).toEqual(mockResponse);
      });

      it('handles unsuccessful API responses', async () => {
        const mockResponse = {
          data: { success: false, message: 'API Error' },
          status: 400,
          config: { url: '/test' },
        };

        mockedAxios.get.mockResolvedValue(mockResponse);

        await expect(materialsAPI.getMaterial('test-id')).rejects.toThrow('API Error');
      });

      it('handles 401 unauthorized responses', async () => {
        const mockError = {
          response: { status: 401, data: { message: 'Unauthorized' } },
          config: { url: '/test' },
        };

        mockedAxios.get.mockRejectedValue(mockError);

        // Mock refresh token flow
        localStorageMock.getItem.mockImplementation((key) => {
          if (key === 'refreshToken') return 'refresh-token';
          if (key === 'accessToken') return 'access-token';
          return null;
        });

        const refreshResponse = {
          data: {
            success: true,
            data: {
              accessToken: 'new-access-token',
              refreshToken: 'new-refresh-token',
            },
          },
        };

        mockedAxios.post.mockResolvedValue(refreshResponse);

        await expect(materialsAPI.getMaterial('test-id')).rejects.toThrow();
      });

      it('redirects to login on refresh failure', async () => {
        const mockError = {
          response: { status: 401 },
          config: { url: '/test' },
        };

        mockedAxios.get.mockRejectedValue(mockError);
        mockedAxios.post.mockRejectedValue(new Error('Refresh failed'));

        await expect(materialsAPI.getMaterial('test-id')).rejects.toThrow();

        expect(localStorageMock.removeItem).toHaveBeenCalledWith('accessToken');
        expect(localStorageMock.removeItem).toHaveBeenCalledWith('refreshToken');
        expect(global.window.location.href).toBe('/login');
      });
    });
  });

  describe('Error Handling', () => {
    it('handles network errors', async () => {
      const networkError = {
        message: 'Network Error',
        response: undefined,
      };

      mockedAxios.get.mockRejectedValue(networkError);

      await expect(materialsAPI.getMaterial('test-id')).rejects.toThrow();
    });

    it('handles 404 errors', async () => {
      const notFoundError = {
        response: { status: 404, data: { message: 'Not found' } },
        message: 'Request failed with status code 404',
      };

      mockedAxios.get.mockRejectedValue(notFoundError);

      await expect(materialsAPI.getMaterial('test-id')).rejects.toThrow();
    });

    it('handles 500 server errors', async () => {
      const serverError = {
        response: { status: 500, data: { message: 'Internal server error' } },
        message: 'Request failed with status code 500',
      };

      mockedAxios.get.mockRejectedValue(serverError);

      await expect(materialsAPI.getMaterial('test-id')).rejects.toThrow();
    });

    it('handles CORS errors', async () => {
      const corsError = {
        message: 'CORS error',
        response: undefined,
      };

      mockedAxios.get.mockRejectedValue(corsError);

      await expect(materialsAPI.getMaterial('test-id')).rejects.toThrow();
    });
  });

  describe('Loading States', () => {
    it('shows loading indicator during requests', async () => {
      // Mock document.getElementById
      const mockLoadingElement = { style: { display: 'none' } };
      document.getElementById = jest.fn().mockReturnValue(mockLoadingElement);

      const mockResponse = { data: { success: true } };
      mockedAxios.get.mockResolvedValue(mockResponse);

      await materialsAPI.getMaterial('test-id');

      expect(document.getElementById).toHaveBeenCalledWith('global-loading');
      expect(mockLoadingElement.style.display).toBe('none');
    });

    it('hides loading indicator on request completion', async () => {
      const mockLoadingElement = { style: { display: 'block' } };
      document.getElementById = jest.fn().mockReturnValue(mockLoadingElement);

      const mockResponse = { data: { success: true } };
      mockedAxios.get.mockResolvedValue(mockResponse);

      await materialsAPI.getMaterial('test-id');

      expect(mockLoadingElement.style.display).toBe('none');
    });
  });

  describe('Audit API', () => {
    describe('getAuditLogs', () => {
      it('fetches audit logs successfully', async () => {
        const mockResponse = {
          data: {
            success: true,
            data: {
              items: [{ id: '1', action: 'create', entityType: 'material' }],
              page: 1,
              pageSize: 10,
              totalCount: 1,
              totalPages: 1,
            },
          },
        };

        mockedAxios.get.mockResolvedValue(mockResponse);

        const result = await auditAPI.getAuditLogs();

        expect(mockedAxios.get).toHaveBeenCalledWith('/audit/logs', { params: undefined });
        expect(result).toEqual(mockResponse);
      });
    });

    describe('exportAuditLogs', () => {
      it('exports audit logs as blob', async () => {
        const mockBlob = new Blob(['test data']);
        const mockResponse = {
          data: mockBlob,
          headers: { 'content-type': 'text/csv' },
        };

        mockedAxios.get.mockResolvedValue(mockResponse);

        const result = await auditAPI.exportAuditLogs({ format: 'csv' });

        expect(mockedAxios.get).toHaveBeenCalledWith('/audit/logs/export', {
          params: { format: 'csv' },
          responseType: 'blob',
        });
        expect(result).toEqual(mockResponse);
      });
    });
  });

  describe('Admin API', () => {
    describe('getAllUsers', () => {
      it('fetches users with search params', async () => {
        const params = { searchTerm: 'test', page: 1, pageSize: 10 };
        const mockResponse = {
          data: {
            success: true,
            data: {
              users: [{ id: '1', email: 'test@test.com' }],
              totalCount: 1,
              page: 1,
              pageSize: 10,
              totalPages: 1,
            },
          },
        };

        mockedAxios.get.mockResolvedValue(mockResponse);

        const result = await adminAPI.getAllUsers(params);

        expect(mockedAxios.get).toHaveBeenCalledWith('/users/search', { params });
        expect(result).toEqual(mockResponse);
      });
    });

    describe('assignRole', () => {
      it('assigns role successfully', async () => {
        const roleData = { userId: 'user-id', role: 'admin' };
        const mockResponse = {
          data: { success: true, message: 'Role assigned successfully' },
        };

        mockedAxios.post.mockResolvedValue(mockResponse);

        const result = await adminAPI.assignRole(roleData);

        expect(mockedAxios.post).toHaveBeenCalledWith('/roles/assign', roleData);
        expect(result).toEqual(mockResponse);
      });
    });
  });
});